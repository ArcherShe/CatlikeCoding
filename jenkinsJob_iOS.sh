#!/bin/bash

GIT_URL="https://github.com/ArcherShe/CatlikeCoding.git"
BRANCH_NAME="${BRANCH}"
PROJECT_PATH="/Users/mac/Jenkins/$JOB_NAME/CatlikeCoding"
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.44f1/Unity.app/Contents/MacOS/Unity"

echo "目标分支: $BRANCH_NAME"
echo "项目路径: $PROJECT_PATH"

if [ ! -d "$PROJECT_PATH" ]; then
    git clone "$GIT_URL" "$PROJECT_PATH"
else
    cd "$PROJECT_PATH" || exit 1

    # 确保 remote 地址正确
    git remote set-url origin "$GIT_URL"

    # 获取远程最新
    git fetch origin

    # 切换到目标分支（如果不存在则创建并跟踪远程分支）
    git checkout "$BRANCH_NAME" || git checkout -b "$BRANCH_NAME" origin/"$BRANCH_NAME"

    # 强制重置本地为远程最新
    git reset --hard "origin/$BRANCH_NAME"

    # 清理未跟踪文件和目录
    //git clean -fd
fi

echo "代码更新完成，当前版本："
cd "$PROJECT_PATH" || exit 1


echo "开始执行 Unity 打包"
echo "平台: $PLATFORM"
echo "渠道: $CHANNEL"

"$UNITY_PATH" \
  -batchmode \
  -projectPath "$PROJECT_PATH" \
  -executeMethod "JenkinsBuild.JenkinsTestBuild" \
  -platform "$PLATFORM" \
  -channel "$CHANNEL" \
  -quit \
  -logFile unity_build.log

UNITY_EXIT_CODE=$?
if [ $UNITY_EXIT_CODE -ne 0 ]; then
  echo "❌ Unity 打包失败，退出码: $UNITY_EXIT_CODE"
  exit $UNITY_EXIT_CODE
fi

echo "✅ Unity 打包完成"