import { http } from '../index'
import type { LoginRequest, LoginResponse, CurrentUser, RefreshTokenRequest, ChangePasswordRequest } from '@/types/auth'

/** 租户查询 DTO */
export interface TenantLookupDto {
  id: string
  name: string
}

/** 认证相关 API */
export const authApi = {
  /** 登录，获取 JWT Token（错误由调用方处理，不走全局拦截器弹消息） */
  login: (data: LoginRequest) =>
    http.post<LoginResponse>('/account/login', data, { _skipErrorHandler: true } as object),

  /** 刷新 Access Token */
  refreshToken: (data: RefreshTokenRequest) =>
    http.post<LoginResponse>('/account/refresh-token', data),

  /** 获取当前登录用户信息 */
  getCurrentUser: () =>
    http.get<CurrentUser>('/account/my-profile'),

  /** 退出登录（服务端吊销令牌，跳过全局错误处理防止死循环） */
  logout: () =>
    http.post('/account/logout', null, { _skipErrorHandler: true } as object),

  /** 修改密码 */
  changePassword: (data: ChangePasswordRequest) =>
    http.post('/account/change-password', data),

  /** 获取租户列表（公开接口，供登录页使用） */
  getTenants: () =>
    http.get<TenantLookupDto[]>('/account/tenants'),
}
