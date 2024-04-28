using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SalvaDocFiscal;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// APIGatewayProxyRequest este tipo foi substituido no código pelo tipo de request abaixo
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        string? bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
        PutObjectResponse response = new PutObjectResponse() ;

        try
        {
            var requestBody = input.Body;
            input.PathParameters.TryGetValue("fileName", out var fileName);

            using (MemoryStream xmlFileStream = new MemoryStream())
            {
                foreach (Byte b in requestBody)
                {
                    xmlFileStream.WriteByte(b);
                }

                PutObjectRequest putRequest = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = xmlFileStream
                };

                response = await client.PutObjectAsync(putRequest);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)response.HttpStatusCode,
                    Body = $"O arquivo {fileName} foi salvo no Bucket"
                };
            }
        }
        catch (AmazonS3Exception ex)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)response.HttpStatusCode,
                Body = $"Ocorreu um erro ao tentar salvar o arquivo no bucket: {ex.Message}" 
            };
        }

        catch (ArgumentNullException ex)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)response.HttpStatusCode,
                Body = $"Ocorreu um erro ao ler o parâmetro fileName: {ex.Message}"
            };
        }

        catch (Exception ex)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)response.HttpStatusCode,
                Body = $"Ocorreu um erro genérico: {ex.Message}"
            };
        }
    }
}
