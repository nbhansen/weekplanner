import { ChangePasswordDTO, UpdateProfileDTO, DeleteUserDTO } from "../hooks/useProfile";
import { coreAxiosInstance } from "./coreAxiosConfig";
import { CoreUserOut, mapCoreUser } from "./coreApiMappers";

export const fetchProfileRequest = async () => {
  return await coreAxiosInstance
    .get<CoreUserOut>(`/users/me`)
    .then((res) => mapCoreUser(res.data))
    .catch(() => {
      throw new Error("Fejl: Kunne ikke hente din profil");
    });
};

export const updateProfileRequest = (data: UpdateProfileDTO) => {
  return coreAxiosInstance
    .put(`/users/me`, {
      first_name: data.firstName,
      last_name: data.lastName,
    })
    .then((res) => mapCoreUser(res.data))
    .catch(() => {
      throw new Error("Fejl: Kunne ikke opdatere din profil");
    });
};

export const changePasswordRequest = (data: ChangePasswordDTO) => {
  return coreAxiosInstance
    .put(`/users/me/password`, {
      old_password: data.oldPassword,
      new_password: data.newPassword,
    })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke opdatere din adgangskode");
    });
};

export const deleteUserRequest = () => {
  return coreAxiosInstance
    .delete(`/users/me`)
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Kunne ikke slette din konto");
    });
};

export const uploadProfileImageRequest = (imageUri: string | null) => {
  if (imageUri === null) {
    throw new Error("FATAL FEJL: Billede er ikke korrekt initialiseret.");
  }

  const imageData = {
    uri: imageUri,
    type: "image/jpeg",
    name: "profile.jpg",
  };

  const formData = new FormData();
  formData.append("file", imageData as unknown as Blob);

  return coreAxiosInstance
    .post(`/users/me/profile-picture`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    })
    .then((res) => res.data)
    .catch((error) => {
      if (error.response) {
        throw new Error(error.message || "Fejl: Kunne ikke uploade profilbillede");
      }
    });
};
