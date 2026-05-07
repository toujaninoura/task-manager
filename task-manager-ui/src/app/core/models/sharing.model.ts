export type TaskShareRole = 'Owner' | 'Editor' | 'Viewer';
export type InvitationStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface CollaboratorResponse {
  id: number;
  userId: number;
  email: string;
  role: TaskShareRole;
  status: InvitationStatus;
  invitedAt: string;
}

export interface InvitationResponse {
  id: number;
  taskId: number;
  taskTitle: string;
  invitedByEmail: string;
  role: TaskShareRole;
  invitedAt: string;
}

export interface InviteCollaboratorRequest {
  email: string;
  role: 'Editor' | 'Viewer';
}
