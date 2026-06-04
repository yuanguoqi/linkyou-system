import { http } from '../index'
import type { LoginRequest, LoginResponse, CurrentUser, RefreshTokenRequest, ChangePasswordRequest } from '@/types/auth'

/** 认证相关 API */
export const authApi = {
  /** 登录，获取 JWT Token */
  login: (data: LoginRequest) =>
    http.post<LoginResponse>('/account/login', data),

  /** 刷新 Access Token */
  refreshToken: (data: RefreshTokenRequest) =>
    http.post<LoginResponse>('/account/refresh-token', data),

  /** 获取当前登录用户信息 */
  getCurrentUser: () =>
    http.get<CurrentUser>('/account/my-profile'),

  /** 退出登录（服务端吊销令牌） */
  logout: () =>
    http.post('/account/logout'),

  /** 修改密码 */
  changePassword: (data: ChangePasswordRequest) =>
    http.post('/account/change-password', data),
}
