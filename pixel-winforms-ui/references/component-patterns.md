# WinForms 像素控件与刷新模式

## 1. 自绘控件基础

自绘控件至少启用：

```csharp
SetStyle(
  ControlStyles.AllPaintingInWmPaint |
  ControlStyles.OptimizedDoubleBuffer |
  ControlStyles.UserPaint |
  ControlStyles.Selectable,
  true);
```

绘制时使用：

```csharp
graphics.SmoothingMode = SmoothingMode.None;
```

先清背景，再绘制硬阴影、外框、主体、状态标记、文字和焦点框。文字使用 `TextRenderer.DrawText`，按钮同时支持鼠标、Enter 和 Space。

## 2. 无边框窗口

- 设置 `FormBorderStyle.None`、`DoubleBuffered=true` 和 `AutoScaleMode.Dpi`。
- 在标题区记录鼠标按下位置，移动时按屏幕坐标差更新窗口位置。
- 显示后调用屏幕边界修正，保证窗口没有完全移出工作区。
- 自己提供关闭按钮，并定义 Enter、Esc 行为。
- 使用统一窗口绘制函数画外框、标题背景和内容边框。

## 3. 按钮

按钮状态顺序：

1. 普通：白底、黑框、硬阴影。
2. 悬停：浅蓝底。
3. 按下：灰色硬反馈。
4. 选中：浅绿底和绿色竖标。
5. 危险悬停：浅红底和红色竖标。
6. 焦点：内部蓝色 2 px 焦点框。

所有状态必须保持文字位置稳定，不能因边框变化跳动。

## 4. 菜单

- 关闭图片边距和系统阴影。
- 菜单项固定宽度和行高。
- 选中项使用浅绿底与黑色矩形框。
- 菜单宽度以最长文字加左右内边距计算，避免右侧大片空白。
- 分隔线、外框和文字使用统一主题颜色。

## 5. 滚动条与日志

- 视觉关键页面不要暴露系统滚动条。
- 自绘上/下箭头、轨道和滑块；滑块最小高度不得过小。
- 日志文本区只负责选择与复制，滚动位置由像素滚动条控制。
- 内容未变化时不重新设置文本，避免滚动位置归零和闪烁。

## 6. 分段进度条

- 使用 10 个或 20 个等宽方块表示进度。
- 外框 4 px，内部块之间保留 2–3 px 间距。
- 同时显示状态文字与百分比，不只依靠颜色。
- 后台线程更新时通过 `BeginInvoke` 回到 UI 线程。

## 7. 原子重绘

后台采集持续运行，界面只控制“何时画”，不控制“是否采集”。

推荐流程：

1. 根据当前数据生成稳定内容签名。
2. 签名未变化时直接返回。
3. 使用刷新防重入标志。
4. 对复杂容器发送 `WM_SETREDRAW = false`。
5. `SuspendLayout` 后一次更新或重建控件。
6. `ResumeLayout`，恢复 `WM_SETREDRAW`，最后 `Invalidate(true)` 与 `Update()`。
7. 在 `finally` 中清除防重入标志。

弹出模态窗口会形成嵌套消息循环，因此刷新入口必须检查防重入，不能假设同一 UI 线程永远不会重入。

## 8. 列表安全

显示数量必须同时受实际数量和布局容量约束：

```csharp
int visibleRows = Math.Min(items.Count, layoutCapacity);
for (int index = 0; index < visibleRows; index++) {
  RenderRow(items[index]);
}
```

错误模式：

```csharp
int visibleRows = layoutCapacity;
```

当布局可容纳 2 行但集合只有 1 条时，错误模式会访问不存在的第 2 条。

对于被截断的列表，应显示“本页 n / 全部 m”，或提供“更多”入口，不能让总数与可见列表看起来互相矛盾。

## 9. 弹窗

- 不使用系统 `MessageBox`。
- 短提示居中；长文本使用只读文本区和像素滚动条。
- 确认与取消按钮位置固定，默认按钮使用选中态。
- 错误弹窗显示用户可理解的信息；调用栈写入诊断日志，不直接堆满普通弹窗。

## 10. DPI 与多屏

- 尽早启用 Per-Monitor V2 DPI；失败时回退到系统 DPI 感知。
- 不把逻辑尺寸与屏幕像素混为一谈。
- 在 100%、150%、200% 下检查标题、按钮、边框、列表和关闭按钮。
- 移动到其他显示器后重新执行屏幕边界修正。
