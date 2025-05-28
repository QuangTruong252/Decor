# Tính năng Sidebar Collapse

## Tổng quan
Đã triển khai thành công tính năng collapse/expand cho sidebar trong admin dashboard với các tính năng sau:

## Tính năng chính

### 1. **Toggle Sidebar**
- **Desktop**: Nút toggle (chevron) trong header sidebar
- **Mobile**: Nút hamburger menu trong navbar
- Smooth animation khi collapse/expand

### 2. **Responsive Design**
- **Desktop (≥768px)**: 
  - Sidebar có thể collapse từ 256px xuống 64px
  - Lưu trạng thái trong localStorage
  - Hiển thị tooltip khi hover vào icon (collapsed state)
- **Mobile (<768px)**:
  - Sidebar luôn collapsed khi load trang
  - Overlay đen khi sidebar mở
  - Click overlay để đóng sidebar
  - Không lưu trạng thái mobile

### 3. **Trạng thái UI**
- **Expanded**: Hiển thị đầy đủ logo + text menu
- **Collapsed**: Chỉ hiển thị icon + nút toggle
- **Mobile**: Sidebar slide từ trái, overlay background

## Cấu trúc Code

### 1. **SidebarProvider** (`components/layouts/SidebarProvider.tsx`)
```typescript
interface SidebarContextType {
  isCollapsed: boolean;
  toggle: () => void;
  collapse: () => void;
  expand: () => void;
}
```

**Tính năng:**
- Context API để quản lý trạng thái global
- Auto-detect mobile và set collapsed = true
- Persist state trong localStorage (chỉ desktop)
- Handle resize events

### 2. **Sidebar Component** (`components/layouts/Sidebar.tsx`)
**Cập nhật:**
- Dynamic width: `w-64` (expanded) ↔ `w-16` (collapsed)
- Toggle button với chevron icons
- Conditional rendering cho text labels
- Tooltip support cho collapsed state
- Smooth transitions với Tailwind

### 3. **AdminLayout** (`components/layouts/AdminLayout.tsx`)
**Cập nhật:**
- Wrap với SidebarProvider
- Mobile overlay logic
- Fixed positioning cho mobile
- Responsive layout adjustments

### 4. **Navbar** (`components/layouts/Navbar.tsx`)
**Cập nhật:**
- Mobile hamburger menu button
- Integration với SidebarContext
- Responsive visibility (`md:hidden`)

## Cách sử dụng

### Trong Component
```typescript
import { useSidebar } from "@/components/layouts/SidebarProvider";

function MyComponent() {
  const { isCollapsed, toggle, collapse, expand } = useSidebar();
  
  return (
    <button onClick={toggle}>
      {isCollapsed ? "Expand" : "Collapse"} Sidebar
    </button>
  );
}
```

### CSS Classes được sử dụng
```css
/* Sidebar width */
.w-64 /* 256px - expanded */
.w-16 /* 64px - collapsed */

/* Transitions */
.transition-all .duration-300 .ease-in-out

/* Mobile positioning */
.fixed .md:relative
.z-50 .md:z-auto
.translate-x-0 .-translate-x-full .md:translate-x-0

/* Overlay */
.fixed .inset-0 .bg-black/50 .z-40 .md:hidden
```

## Breakpoints
- **Mobile**: `< 768px` (md breakpoint)
- **Desktop**: `≥ 768px`

## LocalStorage
- **Key**: `sidebar-collapsed`
- **Value**: `boolean` (JSON)
- **Scope**: Chỉ desktop, mobile không lưu

## Accessibility
- ARIA labels cho toggle buttons
- Keyboard navigation support
- Screen reader friendly
- Focus management

## Browser Support
- Modern browsers với CSS Grid/Flexbox
- LocalStorage support
- CSS transitions support

## Testing
1. **Desktop**: Test toggle button, localStorage persistence
2. **Mobile**: Test hamburger menu, overlay behavior
3. **Responsive**: Test resize behavior
4. **Accessibility**: Test keyboard navigation, screen readers
