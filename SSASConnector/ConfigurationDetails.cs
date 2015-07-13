using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSASConnector
{
    public class ConfigurationDetails
    {
        public List<SharpCloudAttribute> SharpCloudAttributes { get; set; }
        public string MDXQuery { get; set; }
        public string ConnectionString { get; set; }
        public bool DeleteMissingItems { get; set; }
        public string SharpCloudUserName { get; set; }
        public string SharpCloudPassword { get; set; }
        public string SharpCloudURL { get; set; }
        public string SharpCloudStoryID { get; set; }

        public bool ReadConfigurationFile()
        {
            try
            {
                //read configuration file and set up list of attributes
                ConnectionString = ConfigurationManager.AppSettings["CubeConnectionString"];
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: cubeconnectionstring is empty");
                    return false;
                }
                MDXQuery = ConfigurationManager.AppSettings["MDX"];
                if (string.IsNullOrEmpty(MDXQuery))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: MDX is empty");
                    return false;
                }

                SharpCloudURL = ConfigurationManager.AppSettings["SharpCloudURL"];
                if (string.IsNullOrEmpty(SharpCloudURL))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: SharpCloudURL is empty");
                    return false;
                }
                SharpCloudUserName = ConfigurationManager.AppSettings["SharpCloudUserName"];
                if (string.IsNullOrEmpty(SharpCloudUserName))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: SharpCloudUserName is empty");
                    return false;
                }
                SharpCloudPassword = ConfigurationManager.AppSettings["SharpCloudPassword"];
                if (string.IsNullOrEmpty(SharpCloudPassword))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: SharpCloudPassword is empty");
                    return false;
                }
                SharpCloudStoryID = ConfigurationManager.AppSettings["SharpCloudStoryID"];
                if (string.IsNullOrEmpty(SharpCloudStoryID))
                {
                    HelperFunctions.WriteToEventLog("Error reading configuration file: SharpCloudStoryID is empty");
                    return false;
                }

                DeleteMissingItems = false;
                string DeleteMissingItemsString = ConfigurationManager.AppSettings["DeleteMissingItems"];
                if (!string.IsNullOrEmpty(DeleteMissingItemsString))
                {
                    if (DeleteMissingItemsString.ToUpper() == "TRUE")
                    {
                        DeleteMissingItems = true;
                    }
                }

                SharpCloudAttributes = new List<SharpCloudAttribute>();
                //loop
                int settingIndex = 1;
                string fieldSetting;
                bool settingNotFound = false;
                do
                {

                    fieldSetting = ConfigurationManager.AppSettings[string.Format("FieldMapping{0}", settingIndex)];
                    if (!string.IsNullOrEmpty(fieldSetting))
                    {
                        //we have a field, now split
                        string[] settingComponents = fieldSetting.Split(';');
                        if (settingComponents.Length != 3)
                        {
                            HelperFunctions.WriteToEventLog(string.Format("Error reading configuation file, field {0} is not in correct format", fieldSetting));
                            return false;
                        }
                        else
                        {
                            SharpCloudAttribute sharpcloudAttribute = new SharpCloudAttribute();
                            sharpcloudAttribute.CubeMemberName = settingComponents[0];
                            sharpcloudAttribute.SharpCloudAttributeName = settingComponents[1];
                            if (settingComponents[2].ToUpper() != "NUMERIC" && settingComponents[2].ToUpper() != "LIST" && settingComponents[2].ToUpper() != "TEXT")
                            {
                                //
                                HelperFunctions.WriteToEventLog(string.Format("Error reading configuation file for member:{0}, unrecognised data type:{1}", sharpcloudAttribute.CubeMemberName, sharpcloudAttribute.AttributeDataType));
                                return false;
                            }
                            sharpcloudAttribute.AttributeDataType = settingComponents[2].ToUpper();
                            sharpcloudAttribute.CubeTableMappingIndex = 1;
                            SharpCloudAttributes.Add(sharpcloudAttribute);
                        }
                    }
                    else
                    {
                        settingNotFound = true;
                    }
                    if (settingIndex == 1000)
                    {
                        HelperFunctions.WriteToEventLog("Error reading configuation file, please ensure the field settings are in the correct format");
                        return false;
                    }
                    settingIndex++;

                } while (settingNotFound == false);
                return true;
            }
            catch (Exception ex)
            {
                HelperFunctions.WriteToEventLog(string.Format("Error reading configuration file: {0}", ex.Message));
                return false;
            }

        }

        public bool FindDataTableIndexesForAllColumns(DataColumnCollection columns)
        {
            //look at each field in the config file and find the column in the data table
            foreach (var sharpCloudAttribute in SharpCloudAttributes)
            {
                string cubeMemeberToFind;
                if (sharpCloudAttribute.CubeMemberName.Substring(0, 10) == "[Measures]")
                {
                    cubeMemeberToFind = sharpCloudAttribute.CubeMemberName;
                }
                else
                {
                    cubeMemeberToFind = string.Format("{0}.[MEMBER_CAPTION]", sharpCloudAttribute.CubeMemberName);
                }
                for (int columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    DataColumn columnDef = columns[columnIndex];
                    if (columnDef.ColumnName.Contains(cubeMemeberToFind))
                    {
                        sharpCloudAttribute.CubeTableMappingIndex = columnIndex;
                        break;
                    }
                }
            }
            //now check that we can find each column
            foreach (var sharpCloudAttribute in SharpCloudAttributes)
            {
                if (sharpCloudAttribute.CubeTableMappingIndex == -1)
                {
                    //we haven't found this column
                    HelperFunctions.WriteToEventLog(string.Format("Error cannot find memeber: {0} in data table (check MDX and field settings)", sharpCloudAttribute.CubeMemberName));
                    return false;
                }
            }
            return true;
        }

    }
}
