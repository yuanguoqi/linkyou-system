#!/bin/bash
# =====================================================
# Linkyou.System 数据库迁移工具
# 用法: ./scripts/db-migrate.sh [命令] [迁移名称]
# =====================================================

set -e

# 项目路径
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
SRC_DIR="$ROOT_DIR/src"
EF_PROJECT="$SRC_DIR/Linkyou.System.EntityFrameworkCore"
STARTUP_PROJECT="$SRC_DIR/Linkyou.System.Host"

# 颜色
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

# 帮助信息
show_help() {
    echo -e "${CYAN}Linkyou.System 数据库迁移工具${NC}"
    echo ""
    echo "用法: $0 <命令> [参数]"
    echo ""
    echo "命令:"
    echo -e "  ${GREEN}add${NC} <名称>     创建新迁移（如: $0 add AddProducts）"
    echo -e "  ${GREEN}update${NC}          应用所有待执行的迁移"
    echo -e "  ${GREEN}rollback${NC} <名称> 回滚到指定迁移（如: $0 rollback AddMenus）"
    echo -e "  ${GREEN}remove${NC}          移除最后一次未应用的迁移"
    echo -e "  ${GREEN}list${NC}            列出所有迁移"
    echo -e "  ${GREEN}script${NC} [名称]   生成 SQL 脚本（从指定迁移或全部）"
    echo -e "  ${GREEN}status${NC}          显示迁移状态"
    echo -e "  ${GREEN}fresh${NC}           ⚠️ 删除数据库并重新迁移（开发环境）"
    echo ""
    echo "示例:"
    echo "  $0 add AddProducts          # 创建 AddProducts 迁移"
    echo "  $0 update                   # 应用迁移"
    echo "  $0 script AddMenus          # 生成从 AddMenus 开始的 SQL"
    echo "  $0 fresh                    # 重建数据库"
}

# 检查 dotnet ef 工具
check_ef_tools() {
    if ! dotnet ef --version &>/dev/null; then
        echo -e "${RED}错误: 未安装 dotnet-ef 工具${NC}"
        echo "运行: dotnet tool install --global dotnet-ef"
        exit 1
    fi
}

# 创建迁移
cmd_add() {
    local name="$1"
    if [ -z "$name" ]; then
        echo -e "${RED}错误: 请提供迁移名称${NC}"
        echo "用法: $0 add <MigrationName>"
        exit 1
    fi

    echo -e "${CYAN}创建迁移: ${name}${NC}"
    dotnet ef migrations add "$name" \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --output-dir Migrations

    echo -e "${GREEN}✓ 迁移创建成功${NC}"
    echo ""
    echo "迁移文件已生成，包含:"
    echo "  - Migrations/{timestamp}_${name}.cs"
    echo "  - Migrations/{timestamp}_${name}.Designer.cs"
    echo "  - Migrations/LinkyouSystemDbContextModelSnapshot.cs"
    echo ""
    echo -e "${YELLOW}下一步: 运行 '$0 update' 应用迁移${NC}"
}

# 应用迁移
cmd_update() {
    echo -e "${CYAN}应用数据库迁移...${NC}"
    dotnet ef database update \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
    echo -e "${GREEN}✓ 迁移应用完成${NC}"
}

# 回滚迁移
cmd_rollback() {
    local target="$1"
    if [ -z "$target" ]; then
        echo -e "${RED}错误: 请提供目标迁移名称${NC}"
        echo "用法: $0 rollback <MigrationName>"
        echo ""
        echo "可用迁移:"
        cmd_list
        exit 1
    fi

    echo -e "${YELLOW}⚠️ 回滚到: ${target}${NC}"
    dotnet ef database update "$target" \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
    echo -e "${GREEN}✓ 回滚完成${NC}"
}

# 移除迁移
cmd_remove() {
    echo -e "${YELLOW}移除最后一次未应用的迁移...${NC}"
    dotnet ef migrations remove \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
    echo -e "${GREEN}✓ 迁移已移除${NC}"
}

# 列出迁移
cmd_list() {
    echo -e "${CYAN}迁移列表:${NC}"
    dotnet ef migrations list \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
}

# 生成 SQL 脚本
cmd_script() {
    local from="$1"
    local output="$ROOT_DIR/scripts/migration.sql"

    if [ -n "$from" ]; then
        echo -e "${CYAN}生成 SQL 脚本 (从 ${from} 开始)...${NC}"
        dotnet ef migrations script "$from" \
            --project "$EF_PROJECT" \
            --startup-project "$STARTUP_PROJECT" \
            --output "$output" \
            --idempotent
    else
        echo -e "${CYAN}生成完整 SQL 脚本...${NC}"
        dotnet ef migrations script \
            --project "$EF_PROJECT" \
            --startup-project "$STARTUP_PROJECT" \
            --output "$output" \
            --idempotent
    fi

    echo -e "${GREEN}✓ SQL 脚本已生成: ${output}${NC}"
}

# 迁移状态
cmd_status() {
    echo -e "${CYAN}数据库迁移状态:${NC}"
    echo ""

    # 检查数据库连接
    if dotnet ef database update --dry-run \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT" 2>&1 | grep -q "Done"; then
        echo -e "${GREEN}数据库连接: 正常${NC}"
    else
        echo -e "${YELLOW}数据库连接: 可能需要更新${NC}"
    fi

    echo ""
    echo "已应用的迁移:"
    dotnet ef migrations list \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT" 2>/dev/null | grep -E "^\d" || echo "  (无)"

    echo ""
    echo "迁移文件数量:"
    ls -1 "$EF_PROJECT"/Migrations/*.cs 2>/dev/null | grep -v Designer | grep -v Snapshot | wc -l | xargs echo "  "
}

# 重建数据库（开发环境）
cmd_fresh() {
    echo -e "${RED}⚠️ 警告: 此操作将删除数据库并重新创建${NC}"
    read -p "确认执行? (y/N): " confirm
    if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
        echo "已取消"
        exit 0
    fi

    echo -e "${CYAN}删除数据库...${NC}"
    dotnet ef database drop \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --force

    echo -e "${CYAN}重新应用迁移...${NC}"
    dotnet ef database update \
        --project "$EF_PROJECT" \
        --startup-project "$STARTUP_PROJECT"

    echo -e "${GREEN}✓ 数据库重建完成${NC}"
}

# 主入口
check_ef_tools

case "${1:-help}" in
    add)      cmd_add "$2" ;;
    update)   cmd_update ;;
    rollback) cmd_rollback "$2" ;;
    remove)   cmd_remove ;;
    list)     cmd_list ;;
    script)   cmd_script "$2" ;;
    status)   cmd_status ;;
    fresh)    cmd_fresh ;;
    help|*)   show_help ;;
esac
