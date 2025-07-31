using Microsoft.Extensions.Logging;
using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;
using Squad.Service.Utilities;
using System.Text.Json;

namespace Squad.Service.Implementations
{
    public class HabariService : IHabariService
    {
        private readonly ILogger<HabariService> _logger;
        public HabariService(ILogger<HabariService> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// This method is used to look up an account using the encrypted payload provided.
        /// </summary>
        /// <param name="encryptedPayload"></param>
        /// <returns> BaseResponse<LookUpResponse> </returns>
        public async Task<BaseResponse<LookUpResponse>> LookUp(EncryptedPayload encryptedPayload)
        {
            try
            {
                var request = Helper.DecryptAndDeserialize<LookUpRequest>(encryptedPayload.EncryptedRequest);
                if (request == null)
                {
                    return new BaseResponse<LookUpResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid request payload"
                    };
                }
                string accountLookUpEndpoint = Environment.GetEnvironmentVariable("ACCOUNT_LOOKUP");

                var response = await Helper.RequestBankService(accountLookUpEndpoint, HttpMethod.Post, request);

                var accountLookUpResponse = JsonSerializer.Deserialize<BaseResponse<LookUpResponse>>(response);

                return accountLookUpResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LookUp");
                return new BaseResponse<LookUpResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"An error occurred"
                };
            }
        }
        /// <summary>
        /// This method is used to Fund an account using the encrypted payload provided
        /// </summary>
        /// <param name="encryptedPayload"></param>
        /// <returns> BaseResponse<TransferResponse> </returns>
        /// No Sample Response was Provided. So the TransferResponse is empty
        public async Task<BaseResponse<TransferResponse>> Transfer(EncryptedPayload encryptedPayload)
        {
            
            try
            {
                var request = Helper.DecryptAndDeserialize<TransferRequest>(encryptedPayload.EncryptedRequest);
                if (request == null)
                {
                    return new BaseResponse<TransferResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid request payload"
                    };
                }
                string fundTransferEndpoint = Environment.GetEnvironmentVariable("FUND_TRANSFER");

                var response = await Helper.RequestBankService(fundTransferEndpoint, HttpMethod.Post, request);

                var fundTransferResponse = JsonSerializer.Deserialize<BaseResponse<TransferResponse>>(response);

                return fundTransferResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transfer");
                return new BaseResponse<TransferResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"An error occurred"
                };
            }

        }
        /// <summary>
        /// This method is used to re-query a transfer using the encrypted payload provided.
        /// </summary>
        /// <param name="encryptedPayload"></param>
        /// <returns> BaseResponse<RequeryResponse> </returns>
        /// No Sample Response was Provided. So the RequeryResponse is empty
        public async Task<BaseResponse<RequeryResponse>> ReQuery(EncryptedPayload encryptedPayload)
        {
            try
            {
                var request = Helper.DecryptAndDeserialize<ReQueryRequest>(encryptedPayload.EncryptedRequest);
                if (request == null)
                {
                    return new BaseResponse<RequeryResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid request payload"
                    };
                }
                string RequeryTransferEndpoint = Environment.GetEnvironmentVariable("REQUERY_TRANSFER");

                var response = await Helper.RequestBankService(RequeryTransferEndpoint, HttpMethod.Get, request);

                var RequeryTransferResponse = JsonSerializer.Deserialize<BaseResponse<RequeryResponse>>(response);

                return RequeryTransferResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReQuery");
                return new BaseResponse<RequeryResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"An error occurred"
                };
            }
        }
        public async Task<BaseResponse<GetAllTransfersResponse>> GetAllTransfers()
        {
            try
            {
                string GetAllTransfersEndpoint = Environment.GetEnvironmentVariable("GET_ALL_TRANSFERS");

                var response = await Helper.RequestBankService(GetAllTransfersEndpoint, HttpMethod.Get, "");

                var GetAllTransfersResponse = JsonSerializer.Deserialize<BaseResponse<GetAllTransfersResponse>>(response);

                return GetAllTransfersResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllTransfers");
                return new BaseResponse<GetAllTransfersResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"An error occurred"
                };
            }
        }
    }
}
