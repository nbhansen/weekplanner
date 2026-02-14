/**
 * Mapping utilities for GIRAF Core API responses → frontend DTOs.
 *
 * Core uses snake_case; frontend uses camelCase.
 * Core roles: "owner"/"admin"/"member"; frontend: "OrgOwner"/"OrgAdmin"/"OrgMember".
 */

import { ProfileDTO } from "../hooks/useProfile";
import { CitizenDTO, UserDTO } from "../hooks/useOrganisation";
import { GradeDTO } from "../hooks/useGrades";
import { Pictogram } from "../hooks/usePictogram";

// --- Role mapping ---

const CORE_ROLE_MAP: Record<string, "OrgOwner" | "OrgAdmin" | "OrgMember"> = {
  owner: "OrgOwner",
  admin: "OrgAdmin",
  member: "OrgMember",
};

export function mapRoleFromCore(role: string): "OrgOwner" | "OrgAdmin" | "OrgMember" {
  return CORE_ROLE_MAP[role] ?? "OrgMember";
}

// --- User / Profile mapping ---

export interface CoreUserOut {
  id: number;
  username: string;
  email: string;
  first_name: string;
  last_name: string;
  display_name: string;
  is_active: boolean;
  profile_picture: string | null;
}

export function mapCoreUser(u: CoreUserOut): ProfileDTO {
  return {
    email: u.email,
    firstName: u.first_name,
    lastName: u.last_name,
  };
}

// --- Member mapping ---

export interface CoreMemberOut {
  id: number;
  user_id: number;
  username: string;
  first_name: string;
  last_name: string;
  email: string;
  role: string;
}

export function mapCoreMember(m: CoreMemberOut): UserDTO {
  return {
    id: String(m.user_id),
    firstName: m.first_name,
    lastName: m.last_name,
    email: m.email,
    role: mapRoleFromCore(m.role),
  };
}

// --- Citizen mapping ---

export interface CoreCitizenOut {
  id: number;
  first_name: string;
  last_name: string;
  organization_id: number;
}

export function mapCoreCitizen(c: CoreCitizenOut): CitizenDTO {
  return {
    id: c.id,
    firstName: c.first_name,
    lastName: c.last_name,
    activities: [],
  };
}

// --- Grade mapping ---

export interface CoreGradeOut {
  id: number;
  name: string;
  organization_id: number;
}

export function mapCoreGrade(g: CoreGradeOut, citizens: CitizenDTO[] = []): GradeDTO {
  return {
    id: g.id,
    name: g.name,
    citizens,
  };
}

// --- Invitation mapping ---

export interface CoreInvitationOut {
  id: number;
  organization_id: number;
  organization_name: string;
  sender_username: string;
  receiver_username: string;
  status: string;
}

export function mapCoreInvitation(inv: CoreInvitationOut) {
  return {
    id: inv.id,
    organisationId: inv.organization_id,
    organisationName: inv.organization_name,
    senderUsername: inv.sender_username,
    receiverUsername: inv.receiver_username,
    status: inv.status,
  };
}

// --- Pictogram mapping ---

export interface CorePictogramOut {
  id: number;
  name: string;
  image_url: string;
  organization_id: number | null;
}

export function mapCorePictogram(p: CorePictogramOut): Pictogram {
  return {
    id: p.id,
    organizationId: p.organization_id,
    pictogramName: p.name,
    pictogramUrl: p.image_url,
  };
}

// --- Paginated response ---

export interface CorePaginatedResponse<T> {
  items: T[];
  count: number;
}
