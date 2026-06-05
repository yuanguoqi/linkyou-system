/** 当前登录用户信息 */
export interface CurrentUser {
  id: string
  userName: string
  name: string
  surName: string
  email: string
  phoneNumber: string | null
  tenantId: string | null
  roles: string[]
  permissions: string[]
}

/** 登录请求 */
export interface LoginRequest {
  userNameOrEmailAddress: string
  password: string
  rememberMe?: boolean
  tenantId?: string
}

/** 登录响应 */
export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

/** 刷新令牌请求 */
export interface RefreshTokenRequest {
  refreshToken: string
  /** AccessToken 过期后 CurrentUser 为 null，需前端明确传入 userId */
  userId: string
}

/** 修改密码请求 */
export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}
