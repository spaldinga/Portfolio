using DemoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DemoApp;

/// <summary>
/// Service for integrating with Kdp
/// </summary>
public class KdpService : IKdpService
{
    private readonly IJsonWebTokenService _jsonWebTokenService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _client;
    private readonly Uri _baseKdpUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="KdpService"/> class.
    /// </summary>
    /// <param name="jsonWebTokenService">The JSON web token service.</param>
    /// <param name="testObjectOrderService">The test object order service.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="httpClient">The HTTP client.</param>
    public KdpService(IJsonWebTokenService jsonWebTokenService, ITestObjectOrderService testObjectOrderService,
        IConfiguration configuration, HttpClient httpClient)
    {
        _jsonWebTokenService = jsonWebTokenService;
        _configuration = configuration;
        _client = httpClient;
        _baseKdpUrl = new Uri(_configuration.GetRequiredConfigurationValue("Kdp:BaseUrl"));
    }

    /// <inheritdoc />
    public async Task<Models.ProductSpecification> GetProductSpecificationAsync(
        string productNumber12,
        string structureWeek,
        string exterior,
        string interior,
        string factoryCode,
        bool maturityState,
        List<string> optionCodes,
        CancellationToken token = default)
    {
        var productSpecificationRequestModel = new ProductSpecificationRequestModel
        {
            Pc72Z11I = new Pc72Z11I
            {
                ProductSpecification = new ProductSpecification
                {
                    Options = new Options
                    {
                        OptionCode = optionCodes
                    }
                }
            }
        };

        var req = new HttpRequestMessage(HttpMethod.Post,
            CreateProductSpecificationUri(productNumber12, structureWeek, exterior, interior, factoryCode, maturityState))
        {
            Headers =
            {
                { "user-key", _configuration["Kdp:ProductSpecificationUserKey"] },
                { "originatingSystem", _configuration["Kdp:OriginatingSystem"] },
                { "Host", _configuration["Kdp:Host"] },
            },
        };

        await AddJsonWebToken(req, token);

        await req.AddJsonContent(productSpecificationRequestModel);

        var responseMessage = await _client.SendAsync(req, token);
        var response = await responseMessage.Content.ReadAsStringAsync(token);

        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error from KDP with status code {responseMessage.StatusCode} and message: {response}");
        }

        var productSpecificationKdpResponseModel = response.Deserialize<ProductSpecificationKdpResponseModel>();

        return productSpecificationKdpResponseModel.GetProductSpecificationModel();
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> GetVariantSpecificationAsync(
        string testObjectOrderId,
        VariantSpecificationType variantSpecificationType = VariantSpecificationType.Standard,
        CancellationToken token = default)
    {
        var testObjectOrder =
            await _testObjectOrderService.GetTestObjectOrdersWithVariants(testObjectOrderId, token);

        var request = await CreateVariantSpecificationPostRequest(
            CreateVariantSolverUri(variantSpecificationType),
            KdpVariantSpecification.Create(testObjectOrder),
            token);

        return await SendToKdpAsync(request, testObjectOrder, token);
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> GetVariantSpecificationAsync(
        string testObjectOrderId,
        List<LegacyVariantSpecification> variantSpecifications,
        VariantSpecificationType variantSpecificationType = VariantSpecificationType.Standard,
        string structureWeek = null,
        CancellationToken token = default)
    {
        var testObjectOrder =
            await _testObjectOrderService.GetTestObjectOrderWithoutDetailsAsync(testObjectOrderId, token);

        var request = await CreateVariantSpecificationPostRequest(
            CreateVariantSolverUri(variantSpecificationType),
            KdpVariantSpecification.Create(testObjectOrder, variantSpecifications,
                structureWeek ?? testObjectOrder.StructureWeek),
            token);

        return await SendToKdpAsync(request, testObjectOrder, token);
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> GetLatestSavedVariantSpecificationOrDefaultAsync(
        string testObjectOrderId,
        VariantSpecificationType variantSpecificationType = VariantSpecificationType.Standard,
        CancellationToken token = default)
    {
        try
        {
            return await GetLatestSavedVariantSpecificationAsync(testObjectOrderId, variantSpecificationType, token);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> GetLatestSavedVariantSpecificationAsync(
        string testObjectOrderId,
        VariantSpecificationType variantSpecificationType = VariantSpecificationType.Standard,
        CancellationToken token = default)
    {
        var testObjectOrder =
            await _testObjectOrderService.GetTestObjectOrderWithoutDetailsAsync(testObjectOrderId, token);

        var req = await CreateVariantSpecificationRequest(
            HttpMethod.Get,
            CreateVariantSavedSolverUri(variantSpecificationType),
            KdpVariantSpecificationRequest.CreateRequestForLatestSavedVariantSpecification(testObjectOrder), token);

        return await SendToKdpAsync(req, testObjectOrder, token);
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> TranslateVariantSpecificationToDownloadableAsync(
        KdpVariantSpecification standardVariantSpec,
        CancellationToken token = default)
    {
        if (standardVariantSpec == null)
        {
            throw new ArgumentNullException(nameof(standardVariantSpec),
                "Standard variant specification cannot be null.");
        }

        if (!standardVariantSpec.IsStandardConfiguration())
        {
            throw new ArgumentException("Variant specification is not standard format");
        }

        var req = await CreateVariantSpecificationPostRequest(CreateTranslateUri(), standardVariantSpec, token);

        return await SendToKdpAsync(req, null, token);
    }

    /// <inheritdoc />
    public async Task<KdpVariantSpecification> TranslateVariantSpecificationToDownloadableAsync(
        VariantSpecificationVersion variantSpecificationVersion,
        CancellationToken token = default)
    {
        var standardVariantSpec = variantSpecificationVersion
                                    .VariantSpecification?
                                    .ToKdpVariantSpecification();

        if (standardVariantSpec == null)
        {
            throw new ArgumentNullException(nameof(standardVariantSpec),
                "Standard variant specification cannot be null.");
        }

        if (!standardVariantSpec.IsStandardConfiguration())
        {
            throw new ArgumentException("Variant specification is not standard format");
        }

        var req = await CreateVariantSpecificationPostRequest(CreateTranslateUri(), standardVariantSpec, token);

        var kdpVariantSpecification = await SendToKdpAsync(req, variantSpecificationVersion.TestObjectOrder, token);

        return kdpVariantSpecification.TypeCodeDecorator(variantSpecificationVersion.TestObjectOrder)
            .ImmobilizerAssignmentDecorator(variantSpecificationVersion.VariantSpecification);
    }

    private async Task<HttpRequestMessage> CreateVariantSpecificationPostRequest(
        Uri kdpUri, KdpVariantSpecification kdpVariantSpecification, CancellationToken token)
        => await CreateVariantSpecificationRequest(HttpMethod.Post, kdpUri, KdpVariantSpecificationRequest.Create(kdpVariantSpecification), token);

    private async Task<HttpRequestMessage> CreateVariantSpecificationRequest(
        HttpMethod method, Uri kdpUri, KdpVariantSpecificationRequest kdpVariantSpecificationRequest, CancellationToken token)
    {
        var req = new HttpRequestMessage(method, kdpUri);
        req.Headers.Add("user-key", _configuration.GetRequiredConfigurationValue("Kdp:VariantSpecificationUserKey"));
        req.Headers.Host = _configuration.GetRequiredConfigurationValue("Kdp:Host");

        await req.AddJsonContent(kdpVariantSpecificationRequest);

        return await AddJsonWebToken(req, token);
    }

    private async Task<KdpVariantSpecification> SendToKdpAsync(
        HttpRequestMessage request,
        TestObjectOrder testObjectOrder,
        CancellationToken token)
    {
        using var response = await _client.SendAsync(request, token);
        var responseContent = await response.Content.ReadAsStringAsync(token);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Error from KDP with status code {response.StatusCode} and message: {responseContent}");
        }

        var kdpVariantSpecificationResponseModel = responseContent.Deserialize<KdpVariantSpecificationResponseModel>();

        kdpVariantSpecificationResponseModel.Validate();

        var variantSpecification = kdpVariantSpecificationResponseModel.OutputArea.VariantSpecification;
        if (variantSpecification.TypeCode == null
            && testObjectOrder != null
            && !string.IsNullOrWhiteSpace(testObjectOrder.Pno12))
        {
            variantSpecification.TypeCodeDecorator(testObjectOrder);
        }

        return variantSpecification;
    }

    private Uri CreateVariantSolverUri(VariantSpecificationType variantSpecificationType)
    {
        return CreateKdpVariantSpecificationUri("kdp/variantspecification/v100/variant/" + variantSpecificationType.ToUriPath());
    }

    private Uri CreateVariantSavedSolverUri(VariantSpecificationType variantSpecificationType)
    {
        return CreateKdpVariantSpecificationUri("kdp/variantspecification/v100/saved/" + variantSpecificationType.ToUriPath());
    }

    private Uri CreateTranslateUri()
    {
        return CreateKdpVariantSpecificationUri("kdp/variantspecification/v100/translate");
    }

    private Uri CreateKdpVariantSpecificationUri(string path)
    {
        var uriBuilder = new UriBuilder(_baseKdpUrl)
        {
            Path = path,
        };
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["originatingSystem"] = _configuration.GetRequiredConfigurationValue("Kdp:OriginatingSystem");

        uriBuilder.Query = query.ToString() ?? "";

        return uriBuilder.Uri;
    }

    private Uri CreateProductSpecificationUri(
        string productNumber12,
        string structureWeek,
        string exterior,
        string interior,
        string factoryCode,
        bool maturityState)
    {
        var uriBuilder = new UriBuilder(_baseKdpUrl)
        {
            Path = "/kdp/productspecification/v200/",
        };
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        query["productNumber12"] = productNumber12;
        query["structureWeek"] = structureWeek;
        query["colorCode"] = exterior;
        query["upholsteryCode"] = interior;
        query["plantCode"] = factoryCode;
        query["changeOrderStatusPrel"] = maturityState ? "1" : "0";

        uriBuilder.Query = query.ToString() ?? "";

        return uriBuilder.Uri;
    }

    private async Task<HttpRequestMessage> AddJsonWebToken(HttpRequestMessage req, CancellationToken token = default)
    {
        req.Headers.Add(
            "jwtToken",
            await _jsonWebTokenService.GetJwtToken(IJsonWebTokenService.JsonWebTokenKey.Kdp, token)
        );
        return req;
    }
}