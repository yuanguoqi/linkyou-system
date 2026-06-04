// 工具函数：格式化日期时间
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
import 'dayjs/locale/zh-cn'

dayjs.extend(relativeTime)
dayjs.locale('zh-cn')

/**
 * 格式化日期时间
 * @param date ISO 字符串或 Date 对象
 * @param format 格式，默认 YYYY-MM-DD HH:mm:ss
 */
export function formatDateTime(
  date: string | Date | null | undefined,
  format = 'YYYY-MM-DD HH:mm:ss',
): string {
  if (!date) return '-'
  return dayjs(date).format(format)
}

/**
 * 格式化日期
 */
export function formatDate(date: string | Date | null | undefined): string {
  return formatDateTime(date, 'YYYY-MM-DD')
}

/**
 * 相对时间（如：3 分钟前）
 */
export function fromNow(date: string | Date | null | undefined): string {
  if (!date) return '-'
  return dayjs(date).fromNow()
}
