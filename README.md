# ASPFileUploaderAPI
File and Base64 image uploading controller in ASP Net Web API

## File Upload:
Upload a file to the server.

Features:
- Upload multiple files to the server.

Request constraints:
- `content-type HEADER` must be et to: `multipart/form-data`

Query string parameters:
- ```temp=[true |false]```:  
  If set to true, file is uploaded to temporary directory on server, otherwise file is uploaded to default file directory on server. Can be omitted as default is set to ```false```.
- ```keepname=[true|false]```:  
  If set to true, uploaded file keeps original file name, otherwise uploaded file is renamed. Can be omitted as default value is set to ```false```.

> Sample request  
```bash
curl -X POST \
  http://[HTTP_HOST]:[HTTP_PORT]/api/files/ \
  -H 'Host: [HTTP_HOST]:[HTTP_PORT]' \
  -H 'content-length: 11479217' \
  -H 'content-type: multipart/form-data' \
  -F file01=@/C:/Users/apercova/Downloads/apache-tomcat-9.0.21.zip
```
> Sample response
```json
[
    {
        "Name": "file01",
        "FileName": "c7edf9b80bdb4e1f9892136c7ab6a6b1.zip",
        "Length": 11478988,
        "Sha1": "ADA56546CE03EE37C04BE0BAAC533B0EC0A097C8",
        "Uri": "/files/c7edf9b80bdb4e1f9892136c7ab6a6b1.zip",
        "InfoUri": "/api/files/?name=c7edf9b80bdb4e1f9892136c7ab6a6b1.zip&temp=False"
    }
]
```

## Base64 Image upload:
Upload a base64 data image to the server.
Features:
- Upload a Base64 image to the default upload directory.
- Accepts base64 image on both:
  - URL Data Format: `"data" : "data:image/png;base64,iVBORw0KGgoA ..."`
  - data-only image: `"data" : "iVBORw0KGgoA ..."`

Request constraints:
- `content-type HEADER` must be et to: `application/json`

Request payload:
- `name`: `Required` Idicates name for image to be uploaded.
- `data`: `Required` Idicates image data to be uploaded.
- `fileName`: `Optional` but certainly recomended. File extension is taken from filename if it can not be taken from data mime-type.

Query string parameters:
- ```temp=[true |false]```:  
  If set to true, file is uploaded to temporary directory on server, otherwise file is uploaded to default file directory on server. Can be ommited as default is set to ```false```.
- ```keepname=[true|false]```:  
  If set to true, uploaded file keeps original file name, otherwise uploaded file is renamed. Can be omited as default value is set to ```false```.


> Sample request  
```bash
curl -X POST \
  'http://[HTTP_HOST]:[HTTP_PORT]/api/files/base64-images/?keepname=false&temp=false' \
  -H 'Content-Type: application/json' \
  -H 'Host: [HTTP_HOST]:[HTTP_PORT]' \
  -H 'content-length: 24738' \
  -d '[
        {
          "name":"sampleimage",
          "fileName":"sampleimage.png",
          "data":"data:image/png;base64,iVBORw0KGgoA ..."
        }
      ]'
```
> Sample response
```json
[
    {
        "Name": "sampleimage",
        "FileName": "17a59fcfb10b41aa8510a7eafc169b93.png",
        "Length": 18486,
        "Sha1": "69D554D31B61D41BFF62A934C6565986DE9BDAFE",
        "Uri": "/files/17a59fcfb10b41aa8510a7eafc169b93.png",
        "InfoUri": "/api/files/?name=17a59fcfb10b41aa8510a7eafc169b93.png&temp=False"
    }
]
```

### Configuration:
`Web.config` file define the following configuration params:
- `tempDir`: Defines temporary directory for file uploading.
- `uploadDir`: Defines default directory for file uploading.
- `tempPath`: Defines temporary path file uploading.
- `uploadPath`: Defines default path for file uploading.
- `httpRuntime @maxRequestLength`: Defines max request payload length allowed. default is set to `45M`.

```xml
  <appSettings>
    <add key="tempDir" value="~/temp"/>
    <add key="uploadDir" value="~/files"/>
    <add key="tempPath" value="/temp"/>
    <add key="uploadPath" value="/files"/>
  </appSettings>
  <system.web>
    ...
    <!-- maxRequestLength (Kb) = 45 MB -->
    <httpRuntime targetFramework="4.6" maxRequestLength="46080"/>
    ...
  </system.web>
```
