import { Pictogram } from "../hooks/usePictogram";
import { coreAxiosInstance } from "./coreAxiosConfig";
import { CorePaginatedResponse, CorePictogramOut, mapCorePictogram } from "./coreApiMappers";

export const fetchAllPictogramsByOrg = (
  organizationId: number,
  pageSize: number,
  pageNumber: number
): Promise<Pictogram[]> => {
  const offset = (pageNumber - 1) * pageSize;
  return coreAxiosInstance
    .get<CorePaginatedResponse<CorePictogramOut>>(`/pictograms`, {
      params: {
        organization_id: organizationId,
        limit: pageSize,
        offset,
      },
    })
    .then((res) => res.data.items.map(mapCorePictogram))
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at hente piktogrammer");
    });
};

export const deletePictogram = (pictogramId: number): Promise<void> => {
  return coreAxiosInstance
    .delete(`/pictograms/${pictogramId}`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at slette piktogrammet");
    });
};

export const uploadNewPictogram = (formData: FormData): Promise<void> => {
  return coreAxiosInstance
    .post(`/pictograms/upload`, formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med at oprette piktogrammet");
    });
};
