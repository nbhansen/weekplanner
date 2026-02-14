import { OrgOverviewDTO } from "../hooks/useOrganisationOverview";
import { coreAxiosInstance } from "./coreAxiosConfig";
import { CorePaginatedResponse } from "./coreApiMappers";

export const fetchAllOrganisationsRequest = (): Promise<OrgOverviewDTO[]> => {
  return coreAxiosInstance
    .get<CorePaginatedResponse<OrgOverviewDTO>>(`/organizations`, {
      params: { limit: 1000 },
    })
    .then((res) => res.data.items)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at hente organisationer");
    });
};

export const deleteOrganisationRequest = (organisationId: number) => {
  return coreAxiosInstance
    .delete(`/organizations/${organisationId}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at slette organisationen");
    });
};

export const createOrganisationsRequest = (orgName: string): Promise<OrgOverviewDTO> => {
  return coreAxiosInstance
    .post(`/organizations`, { name: orgName })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at oprette organisation");
    });
};
