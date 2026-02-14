import { useQuery } from "@tanstack/react-query";
import { fetchPictogramById } from "../apis/pictogramAPI";

export function usePictogramById(id: number | undefined) {
  return useQuery({
    queryKey: ["pictogram", id],
    queryFn: () => fetchPictogramById(id!),
    enabled: !!id,
    staleTime: Infinity,
  });
}
