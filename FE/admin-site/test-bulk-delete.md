# Test Bulk Delete Feature for Products

## Tính năng đã triển khai:

### 1. **API Service** (`services/products.ts`)
- ✅ Thêm function `bulkDeleteProducts(ids: number[])`
- ✅ Gọi API endpoint `/api/Products/bulk` với method DELETE
- ✅ Gửi payload `{ ids: number[] }` theo BulkDeleteDTO

### 2. **Hook** (`hooks/useProducts.ts`)
- ✅ Thêm hook `useBulkDeleteProducts()`
- ✅ Xử lý success/error với toast notifications
- ✅ Invalidate queries sau khi delete thành công

### 3. **Bulk Delete Component** (`components/products/BulkDeleteButton.tsx`)
- ✅ Component riêng để xử lý bulk delete
- ✅ Hiển thị confirmation dialog trước khi delete
- ✅ Hiển thị loading state khi đang xử lý
- ✅ Chỉ hiển thị khi có items được chọn

### 4. **Products Page** (`app/(admin)/products/page.tsx`)
- ✅ Thêm state `selectedProducts` để quản lý selection
- ✅ Thêm checkbox "Select All" ở header
- ✅ Thêm checkbox cho từng product row
- ✅ Thêm BulkDeleteButton ở header
- ✅ Xử lý logic select/deselect products

## Cách test:

### Test Case 1: Select All và Bulk Delete
1. Mở trang Products
2. Click checkbox "Select All" ở header
3. Verify tất cả products được chọn
4. Click button "Delete X items"
5. Confirm trong dialog
6. Verify products được xóa thành công

### Test Case 2: Select Individual và Bulk Delete
1. Mở trang Products
2. Chọn một vài products bằng checkbox riêng lẻ
3. Click button "Delete X items"
4. Confirm trong dialog
5. Verify chỉ những products được chọn bị xóa

### Test Case 3: Deselect
1. Select một vài products
2. Deselect một số products
3. Verify button hiển thị đúng số lượng còn lại
4. Bulk delete và verify chỉ những items còn được chọn bị xóa

### Test Case 4: Error Handling
1. Disconnect internet hoặc stop API server
2. Thử bulk delete
3. Verify error toast hiển thị
4. Verify UI không bị crash

## UI/UX Features:

- ✅ Checkbox indeterminate state khi một phần được chọn
- ✅ Button chỉ hiển thị khi có items được chọn
- ✅ Loading state với spinner
- ✅ Confirmation dialog với thông tin chi tiết
- ✅ Toast notifications cho success/error
- ✅ Responsive design
- ✅ Accessibility với proper ARIA labels

## API Integration:

- ✅ Sử dụng đúng endpoint `/api/Products/bulk`
- ✅ Gửi đúng payload format theo BulkDeleteDTO
- ✅ Xử lý response và error cases
- ✅ Refresh data sau khi delete thành công
