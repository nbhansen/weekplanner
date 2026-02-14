import { useAuthentication } from "../providers/AuthenticationProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  acceptInvitationRequest,
  createInvitationRequest,
  fetchInvitationByUserRequest,
} from "../apis/invitationAPI";

export default function useInvitation() {
  const { userId } = useAuthentication();
  const queryClient = useQueryClient();
  const queryKey = [userId!, "Invitation"];

  const fetchByUser = useQuery({
    queryFn: async () => fetchInvitationByUserRequest(),
    queryKey,
    enabled: !!userId,
  });

  const acceptInvitation = useMutation({
    mutationFn: ({ invitationId, isAccepted }: { invitationId: number; isAccepted: boolean }) =>
      acceptInvitationRequest(invitationId, isAccepted),

    onMutate: async ({ invitationId }) => {
      await queryClient.cancelQueries({ queryKey });
      const previousInvitations = queryClient.getQueryData(queryKey);

      queryClient.setQueryData(queryKey, (oldData: any) =>
        oldData.filter((invitation: any) => invitation.id !== invitationId)
      );

      return { previousInvitations };
    },

    onError: (_error, _variables, context) => {
      if (context?.previousInvitations) {
        queryClient.setQueryData(queryKey, context.previousInvitations);
      }
    },
    onSuccess() {
      queryClient.invalidateQueries({ queryKey: [userId, "OrganisationOverview"] });
    },
  });

  const createInvitation = useMutation({
    mutationFn: async ({
      orgId,
      receiverEmail,
    }: {
      orgId: number;
      receiverEmail: string;
      senderId?: string;
    }) => createInvitationRequest(orgId, receiverEmail),
  });

  return {
    fetchByUser,
    acceptInvitation,
    createInvitation,
    isSuccess: fetchByUser.isSuccess,
  };
}
