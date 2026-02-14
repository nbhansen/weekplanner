import { GradeDTO } from "../hooks/useGrades";
import { CitizenDTO, FullOrgDTO } from "../hooks/useOrganisation";
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

export const addCitizenToGradeRequest = (
  citizenIds: number[],
  gradeId: number,
  _orgId: number
): Promise<GradeDTO> => {
  return coreAxiosInstance
    .post(`/grades/${gradeId}/citizens/add`, { citizen_ids: citizenIds })
    .then((res) => mapCoreGrade(res.data))
    .catch(() => {
      throw new Error("Fejl: Kunne ikke tilføje borger til klasse");
    });
};

export const fetchCitizenById = (citizenId: number): Promise<CitizenDTO> => {
  return coreAxiosInstance
    .get<CoreCitizenOut>(`/citizens/${citizenId}`)
    .then((res) => mapCoreCitizen(res.data))
    .catch(() => {
      throw new Error(`Fejl: Kunne ikke hente borger.`);
    });
};

export const fetchOrganisationFromGradeRequest = async (gradeId: number): Promise<FullOrgDTO> => {
  if (gradeId === null || gradeId === undefined) {
    throw new Error("FATAL FEJL: Klasse-ID er ikke korrekt initialiseret i din session.");
  }

  // Get grade to find organization_id, then compose the full org
  const gradeRes = await coreAxiosInstance
    .get<CoreGradeOut>(`/grades/${gradeId}`)
    .catch(() => {
      throw new Error(`Fejl: Kunne ikke hente klasse.`);
    });

  const orgId = gradeRes.data.organization_id;

  const [orgRes, membersRes, citizensRes, gradesRes] = await Promise.all([
    coreAxiosInstance.get(`/organizations/${orgId}`),
    coreAxiosInstance.get<CorePaginatedResponse<CoreMemberOut>>(
      `/organizations/${orgId}/members`,
      { params: { limit: 1000 } }
    ),
    coreAxiosInstance.get<CorePaginatedResponse<CoreCitizenOut>>(
      `/organizations/${orgId}/citizens`,
      { params: { limit: 1000 } }
    ),
    coreAxiosInstance.get<CorePaginatedResponse<CoreGradeOut>>(
      `/organizations/${orgId}/grades`,
      { params: { limit: 1000 } }
    ),
  ]).catch(() => {
    throw new Error(`Fejl: Kunne ikke hente organisation.`);
  });

  return {
    id: orgRes.data.id,
    name: orgRes.data.name,
    users: membersRes.data.items.map(mapCoreMember),
    citizens: citizensRes.data.items.map(mapCoreCitizen),
    grades: gradesRes.data.items.map((g: CoreGradeOut) => mapCoreGrade(g)),
  };
};

export const removeCitizenFromGradeRequest = (
  citizenIds: number[],
  gradeId: number,
  _orgId: number
): Promise<GradeDTO> => {
  return coreAxiosInstance
    .post(`/grades/${gradeId}/citizens/remove`, { citizen_ids: citizenIds })
    .then((res) => mapCoreGrade(res.data))
    .catch(() => {
      throw new Error("Fejl: Kunne ikke fjerne borger fra klasse");
    });
};

export const createNewGradeRequest = async (gradeName: string, orgId: number): Promise<GradeDTO> => {
  return coreAxiosInstance
    .post(`/organizations/${orgId}/grades`, { name: gradeName })
    .then((res) => mapCoreGrade(res.data))
    .catch((error) => {
      throw new Error(error.message || "Fejl: Kunne ikke oprette klasse");
    });
};

export const updateGradeRequest = async (gradeId: number, newName: string, _orgId: number) => {
  return coreAxiosInstance
    .patch(`/grades/${gradeId}`, { name: newName })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke opdatere klasse");
    });
};

export const deleteGradeRequest = async (gradeId: number, _orgId: number) => {
  return coreAxiosInstance
    .delete(`/grades/${gradeId}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke slette din klasse");
    });
};
