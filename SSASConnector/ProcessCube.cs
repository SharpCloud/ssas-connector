using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AnalysisServices.AdomdClient;
using SC.API.ComInterop;
using SC.API.ComInterop.Models;

namespace SSASConnector
{
    public class ProcessCube
    {
       private ConfigurationDetails _configDetails;

        public ProcessCube() { }

        public ProcessCube(ConfigurationDetails configDetails)
        {
            _configDetails = configDetails;
        }

        public bool ConnectAndProcessCube()
        {

            try
            {

            using (AdomdConnection cubeConnection = new AdomdConnection(_configDetails.ConnectionString))
            {
                cubeConnection.Open();

                using (AdomdCommand cmd = new AdomdCommand(_configDetails.MDXQuery, cubeConnection))
                {
                    Console.WriteLine("Connected to the cube");

                    DataSet ds = new DataSet();
                    ds.EnforceConstraints = false;
                    ds.Tables.Add();
                    DataTable dt = ds.Tables[0];
                    dt.Load(cmd.ExecuteReader());

                    Console.WriteLine(string.Format("Data loaded from cube: {0} rows", dt.Rows.Count));

                    //we have the data table, now find the index of the fields we want 
                    _configDetails.FindDataTableIndexesForAllColumns(dt.Columns);

                    int rowCount = dt.Rows.Count;
                    int colCount = _configDetails.SharpCloudAttributes.Count;

                    //arrayValues is the array that we pass to the SharpCloud Lib dll in order to update the story
                    string[,] arrayValues = new string[rowCount + 1, colCount];

                    //the first row must be the column headings, eg:
                    //arrayValues[0, 0] = "ExternalID";
                    //arrayValues[0, 1] = "Name";

                    for (int i = 0; i < _configDetails.SharpCloudAttributes.Count; i++)
                    {
                        if (_configDetails.SharpCloudAttributes[i].SharpCloudAttributeName.ToUpper() == "NAME" ||
                            _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName.ToUpper() == "CATEGORY" ||
                            _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName.ToUpper() == "EXTERNALID" ||
                            _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName.ToUpper() == "EXTERNAL ID")
                        {
                            //built in column, don't change
                            arrayValues[0, i] = _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName;
                        }
                        else if (_configDetails.SharpCloudAttributes[i].AttributeDataType.ToUpper() == "LIST")
                        {
                            arrayValues[0, i] = string.Format("L__{0}", _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName);
                        }
                        else if (_configDetails.SharpCloudAttributes[i].AttributeDataType.ToUpper() == "NUMERIC")
                        {
                            arrayValues[0, i] = string.Format("N__{0}", _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName);
                        }
                        else if (_configDetails.SharpCloudAttributes[i].AttributeDataType.ToUpper() == "DATE")
                        {
                            arrayValues[0, i] = string.Format("D__{0}", _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName);
                        }
                        else if (_configDetails.SharpCloudAttributes[i].AttributeDataType.ToUpper() == "TEXT")
                        {
                            arrayValues[0, i] = string.Format("T__{0}",_configDetails.SharpCloudAttributes[i].SharpCloudAttributeName);
                        }
                        else
                        {
                            arrayValues[0, i] = _configDetails.SharpCloudAttributes[i].SharpCloudAttributeName;
                        }
                        _configDetails.SharpCloudAttributes[i].ArrayColumnIndex = i;
                    }

                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        DataRow dataRow = dt.Rows[rowIndex];

                        for (int attIndex = 0; attIndex < _configDetails.SharpCloudAttributes.Count; attIndex++)
                        {
                            SharpCloudAttribute sharpCloudAttribute = _configDetails.SharpCloudAttributes[attIndex];
                            string cubeValueString = dataRow.ItemArray[sharpCloudAttribute.CubeTableMappingIndex].ToString();
                            if (sharpCloudAttribute.AttributeDataType == "NUMERIC")
                            {
                                double numericValue;
                                if (double.TryParse(cubeValueString, out numericValue))
                                {
                                    arrayValues[rowIndex + 1, sharpCloudAttribute.ArrayColumnIndex] = numericValue.ToString();
                                }
                                else
                                {
                                    arrayValues[rowIndex + 1, sharpCloudAttribute.ArrayColumnIndex] = "(null)";
                                }
                            }
                            else
                            {
                                //must be a string
                                arrayValues[rowIndex + 1, sharpCloudAttribute.ArrayColumnIndex] = cubeValueString;
                            }
                        }

                    }

                    string errorMessage;

                    Console.WriteLine(string.Format("Connecting to SharpCloud"));

                    var client = new SharpCloudApi(_configDetails.SharpCloudUserName, _configDetails.SharpCloudPassword, _configDetails.SharpCloudURL);
                    Story rm = client.LoadStory(_configDetails.SharpCloudStoryID);
                    Console.WriteLine(string.Format("Loaded story from SharpCloud, now updating"));
                    if (rm.UpdateStoryWithArray(arrayValues, _configDetails.DeleteMissingItems, out errorMessage))
                    {
                        rm.Save();
                        Console.WriteLine("***Update Complete***");
                        Console.WriteLine("");
                        Console.WriteLine("Press any key to close this window");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Error updating story: " + errorMessage);
                        Console.WriteLine("");
                        Console.WriteLine("Press any key to close this window");
                        Console.ReadLine();
                    }

                }

            };
            return true;

            }
            catch (Exception ex)
            {

                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("Press any key to close this window");
                Console.ReadLine();
                return false;
            }

        }

    }
}
