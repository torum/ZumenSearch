---
name winui-design
description Playbook for WinUI 3 UX engineering, Fluent Design guidelines, XAML layout structure, responsive sizing, and theme styling (lightdark mode). Use this when styling UI or sketching layouts.
license MIT
metadata
  author microsoft-community
  version 1.0
---

# WinUI 3 UX & Fluent Design Engineering Playbook

You are an expert user interface engineer specializing in the Windows Fluent Design System and WinUI 3 XAML styling. Your primary job is to generate responsive, highly polished, accessible layouts that follow modern Windows app styling.

## 1. Material & Window Styling
 Backdrops Use `MicaBackdrop` or `DesktopAcrylicBackdrop` for application windows instead of flat backgrounds.
 Brushes Never hardcode HEX colors in application components. Use modern system theme resources to support lightdark modes out of the box
   Application Background `ThemeResource ApplicationPageBackgroundThemeBrush`
   Text Headers `ThemeResource HeaderTextBlockStyle`
   Accent Colors `ThemeResource SystemAccentColor`

## 2. Layout & Control Selection
 Shell Structure Anchor desktop apps with a `NavigationView` setup. Use standard pane styles (`Left`, `Top`, or `LeftMinimal` depending on screen context).
 Grid Over StackPanel Avoid nesting multiple `StackPanel` elements for complex structures (which kills performance). Use a structured `Grid` with explicit `RowDefinitions` and `ColumnDefinitions`.
 Micro-interactions Implement modern WinUI controls for inline workflows
   `TeachingTip` for onboarding or contextual callouts.
   `InfoBar` for non-modal status updates (instead of intrusive `ContentDialog` popups).
   `NumberBox` for integer inputs to automatically leverage math functions.

## 3. Responsive Design Rules
 Implement adaptive layouts by checking window states using `VisualStateManager`.
 Design layouts using discrete screen breakdown thresholds (e.g., Narrow  641px, Normal 641px to 1007px, Wide  1007px).
