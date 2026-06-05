<script setup lang="ts">
import { onMounted, ref } from 'vue'

const emit = defineEmits<{ code: [value: string] }>()

const canvasRef = ref<HTMLCanvasElement>()

const CHARS = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789'

function randomChar() {
  return CHARS[Math.floor(Math.random() * CHARS.length)]
}

function randomInt(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1)) + min
}

function randomColor(minL = 30, maxL = 70) {
  const h = randomInt(0, 360)
  const s = randomInt(50, 80)
  const l = randomInt(minL, maxL)
  return `hsl(${h},${s}%,${l}%)`
}

function generate() {
  const canvas = canvasRef.value
  if (!canvas) return
  const ctx = canvas.getContext('2d')!
  const w = canvas.width
  const h = canvas.height

  // 背景
  ctx.fillStyle = '#1e2740'
  ctx.fillRect(0, 0, w, h)

  // 干扰线
  for (let i = 0; i < 4; i++) {
    ctx.beginPath()
    ctx.moveTo(randomInt(0, w), randomInt(0, h))
    ctx.lineTo(randomInt(0, w), randomInt(0, h))
    ctx.strokeStyle = randomColor(40, 60)
    ctx.lineWidth = 1
    ctx.stroke()
  }

  // 干扰点
  for (let i = 0; i < 40; i++) {
    ctx.beginPath()
    ctx.arc(randomInt(0, w), randomInt(0, h), 1, 0, Math.PI * 2)
    ctx.fillStyle = randomColor(50, 70)
    ctx.fill()
  }

  // 生成4位验证码
  let code = ''
  for (let i = 0; i < 4; i++) {
    code += randomChar()
  }

  // 绘制字符
  for (let i = 0; i < 4; i++) {
    const char = code[i]
    const x = 16 + i * 26
    const y = randomInt(22, 30)
    const angle = (randomInt(-25, 25) * Math.PI) / 180

    ctx.save()
    ctx.translate(x, y)
    ctx.rotate(angle)
    ctx.font = `bold ${randomInt(18, 22)}px 'Courier New', monospace`
    ctx.fillStyle = randomColor(65, 95)
    ctx.shadowColor = randomColor(50, 80)
    ctx.shadowBlur = 4
    ctx.fillText(char, 0, 0)
    ctx.restore()
  }

  emit('code', code)
}

defineExpose({ generate })

onMounted(() => generate())
</script>

<template>
  <canvas ref="canvasRef" :width="120" :height="40" />
</template>
