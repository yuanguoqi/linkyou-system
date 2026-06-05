import { http } from '../index'

// ── Setting DTOs ───────────────────────────────────────
export interface SettingDto {
  name: string
  value: string
  displayName: string
  description: string
  isEncrypted: boolean
}

export interface UpdateSettingDto {
  name: string
  value: string
}

export interface GetSettingsInput {
  providerName?: string
  providerKey?: string
}

// ── API ────────────────────────────────────────────────
const BASE_URL = '/api/setting-management/settings'

export const settingApi = {
  get: (params: GetSettingsInput) =>
    http.get<SettingDto[]>(`${BASE_URL}/by-global`, { params }),

  update: (data: UpdateSettingDto[]) =>
    http.put(BASE_URL, data),
}
