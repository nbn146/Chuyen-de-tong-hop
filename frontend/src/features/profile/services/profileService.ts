import { apiClient } from '../../../shared/services/apiClient'

export interface Profile {
  userID: number
  email: string
  fullName: string
  phone?: string
  avatarURL?: string
}

export interface UpdateProfileRequest {
  fullName: string
  phone?: string
  avatarURL?: string
}

export const profileService = {
  get: (): Promise<Profile> => apiClient<Profile>('/profile'),
  update: (request: UpdateProfileRequest): Promise<Profile> => apiClient<Profile>('/profile', {
    method: 'PUT',
    body: JSON.stringify(request),
  }),
  forgotPassword: (email: string): Promise<{ message: string; resetToken: string }> =>
    apiClient<{ message: string; resetToken: string }>('/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify({ email }),
    }),
  resetPassword: (token: string, newPassword: string): Promise<{ message: string }> =>
    apiClient<{ message: string }>('/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify({ token, newPassword }),
    }),
}
