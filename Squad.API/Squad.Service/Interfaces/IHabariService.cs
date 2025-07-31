using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;

namespace Squad.Service.Interfaces
{
    public interface IHabariService
    {
        Task<BaseResponse<GetAllTransfersResponse>> GetAllTransfers();
        Task<BaseResponse<LookUpResponse>> LookUp(EncryptedPayload encryptedPayload);
        Task<BaseResponse<RequeryResponse>> ReQuery(EncryptedPayload encryptedPayload);
        Task<BaseResponse<TransferResponse>> Transfer(EncryptedPayload encryptedPayload);
    }
}
