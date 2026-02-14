import { coreAxiosInstance } from "./coreAxiosConfig";
import { CoreInvitationOut, CorePaginatedResponse, mapCoreInvitation } from "./coreApiMappers";

export const fetchInvitationByUserRequest = async () => {
  return await coreAxiosInstance
    .get<CorePaginatedResponse<CoreInvitationOut>>(`/invitations/received`, {
      params: { limit: 1000 },
    })
    .then((res) => res.data.items.map(mapCoreInvitation))
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med hentning af invitation");
    });
};

export const acceptInvitationRequest = (invitationId: number, isAccepted: boolean) => {
  const action = isAccepted ? "accept" : "reject";
  return coreAxiosInstance
    .post(`/invitations/${invitationId}/${action}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at acceptere invitation");
    });
};

export const createInvitationRequest = (orgId: number, receiverEmail: string) => {
  return coreAxiosInstance
    .post(`/organizations/${orgId}/invitations`, { receiver_email: receiverEmail })
    .then((res) => res.data)
    .catch((error) => {
      if (error.response) {
        const detail = error.response.data?.detail;
        throw new Error(detail || "Fejl: Der opstod et problem med at oprette invitation");
      }
    });
};
