# ssas-connector
Connects to a SSAS (SQL Server Analysis Services) Cube and populates a SharpCloud story with data from the cube.

#### App.config file settings

**SharpCloudURL**
The URL of the SharpCloud instance, eg. https://my.sharpcloud.com

**sharpCloudStoryID**
The ID of the SharpCloud story that will be updated with data from the spreadsheet

**SharpCloudUsername**
**SharpCloudPassword**
The username and password of a user that has edit rights on the SharpCloud story

**CubeConnectionString**
The connection string detailing how to connect to the SSAS cube, please complete the Data Source and Initial Catalog parts. This example assumes Integrated Security.

**MDX**
The MDX query to retrieve the relevant data from the cube.

**DeleteMissingItems**
Flag (TRUE or FALSE) to indicate whether items are deleted from the source story if they no longer exist in the cube.

**FieldMappings**
The Feed must be configured to know how to map between data in the SSAS cube and data in the SharpCloud story. This is defined in the FieldMappings section of the configuration file. Each setting is given a name of FieldSetting plus a number (the number is used so that each configuration key has a unique name and can be read by the application). The value of the setting is in three parts, separated by a semi-colon:

* The SSAS cube dimension/measure (as specified in the MDX query), usually this is enclosed within square brackets.
* The SharpCloud field/attribute name. Sometimes this may be a built-in SharpCloud field such as Name or Category, in which case special logic is applied (see Appendix 3 for a full list of built-in fields).
* The data-type of the field. This determines the type of SharpCloud attribute that will be created (if it does not already exist). Can be one of the following values:
 * Text
 * List
 * Numeric
 * Date
 
**Full list of built-in SharpCloud field names**
* Name: The name of the item.
* ExternalID or External ID: The external identifier of the item Description The description of the item.
* Start: The start date of the item.
* Category The category name (if the category does not exist than a new one will be created).
* Duration: The duration, in days, of the item.
* ClickActionURL A URL to be applied when clicking on the item.
* Image: The image identifier for the main item image, this is in the format of the GUID.
* Published DO NOT USE: Whether the item is published or not.
* Likes: DO NOT USE: Read only field of number likes for the item. 
* Dislikes DO NOT USE: Read only field of number dislikes for the item.
* Tags: Tags to create for the item.
