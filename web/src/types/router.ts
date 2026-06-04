/** 路由元信息扩展 */
export interface RouteMeta {
  /** 页面标题（显示在 breadcrumb 和 tab） */
  title: string
  /** 菜单图标（Element Plus 图标名） */
  icon?: string
  /** 访问所需权限（对应后端权限常量） */
  permission?: string
  /** 是否隐藏（不显示在左侧菜单中） */
  hidden?: boolean
  /** 是否固定在 tab 栏（不可关闭） */
  affix?: boolean
  /** 是否缓存页面（keep-alive） */
  keepAlive?: boolean
}

/** 菜单项（用于渲染左侧导航） */
export interface MenuItem {
  path: string
  title: string
  icon?: string
  children?: MenuItem[]
}
