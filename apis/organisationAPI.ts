import { coreAxiosInstance } from "./coreAxiosConfig";
import {
  CoreCitizenOut,
  CoreGradeOut,
  CoreMemberOut,
  CorePaginatedResponse,
  mapCoreCitizen,
  mapCoreGrade,
  mapCoreMember,
} from "./coreApiMappers";
import { FullOrgDTO } from "../hooks/useOrganisation";

export const fetchOrganisationRequest = async (organisationId: number): Promise<FullOrgDTO> => {
  const [orgRes, membersRes, citizensRes, gradesRes] = await Promise.all([
    coreAxiosInstance.get(`/organizations/${organisationId}`),
    coreAxiosInstance.get<CorePaginatedResponse<CoreMemberOut>>(
      `/organizations/${organisationId}/members`,
      { params: { limit: 1000 } }
    ),
    coreAxiosInstance.get<CorePaginatedResponse<CoreCitizenOut>>(
      `/organizations/${organisationId}/citizens`,
      { params: { limit: 1000 } }
    ),
    coreAxiosInstance.get<CorePaginatedResponse<CoreGradeOut>>(
      `/organizations/${organisationId}/grades`,
      { params: { limit: 1000 } }
    ),
  ]).catch(() => {
    throw new Error("Fejl: Der opstod et problem med anmodningen");
  });

  return {
    id: orgRes.data.id,
    name: orgRes.data.name,
    users: membersRes.data.items.map(mapCoreMember),
    citizens: citizensRes.data.items.map(mapCoreCitizen),
    grades: gradesRes.data.items.map((g: CoreGradeOut) => mapCoreGrade(g)),
  };
};

export const createCitizenRequest = (
  firstname: string,
  lastName: string,
  orgId: number
): Promise<number> => {
  return coreAxiosInstance
    .post(`/organizations/${orgId}/citizens`, {
      first_name: firstname,
      last_name: lastName,
    })
    .then((res) => res.data.id)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med anmodningen");
    });
};

export const deleteCitizenRequest = (_orgId: number, citizenId: number) => {
  return coreAxiosInstance
    .delete(`/citizens/${citizenId}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med anmodningen");
    });
};

export const deleteMemberRequest = (orgId: number, memberId: string) => {
  return coreAxiosInstance
    .delete(`/organizations/${orgId}/members/${memberId}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med anmodningen");
    });
};

export const updateCitizenRequest = (citizenId: number, firstName: string, lastName: string) => {
  return coreAxiosInstance
    .patch(`/citizens/${citizenId}`, {
      first_name: firstName,
      last_name: lastName,
    })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med anmodningen");
    });
};

export const updateOrganisationRequest = (orgId: number, name: string) => {
  return coreAxiosInstance
    .patch(`/organizations/${orgId}`, { name })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke opdatere organisation");
    });
};

export const makeAdminRequest = (orgId: number, userId: string) => {
  return coreAxiosInstance
    .patch(`/organizations/${orgId}/members/${userId}`, { role: "admin" })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke gøre brugeren til admin");
    });
};

export const removeAdminRequest = (orgId: number, userId: string) => {
  return coreAxiosInstance
    .patch(`/organizations/${orgId}/members/${userId}`, { role: "member" })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke fjerne brugerens admin-rettigheder");
    });
};
