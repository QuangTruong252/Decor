### Thiết kế Kỹ thuật Chi tiết: Admin Dashboard cho E-commerce

**Tổng quan:** Xây dựng một trang quản trị (Admin Dashboard) cho phép quản lý các khía cạnh khác nhau của một hệ thống E-commerce. Giao diện sẽ được xây dựng bằng Next.js, Tailwind CSS, và thư viện component Shadcn UI. Dữ liệu sẽ được đồng bộ hóa thông qua các API backend của hệ thống E-commerce, với thông tin và đặc tả chi tiết của các API này được lấy qua MCP.

**Mục đích:** Cung cấp cho quản trị viên một công cụ trực quan và hiệu quả để theo dõi, quản lý sản phẩm, đơn hàng, khách hàng và các hoạt động khác của cửa hàng trực tuyến.

**Các Thực thể Chính cần Quản lý (Giả định ban đầu, có thể mở rộng):**

*   Sản phẩm (Products)
*   Danh mục sản phẩm (Categories)
*   Đơn hàng (Orders)
*   Khách hàng (Customers)
*   Tài khoản quản trị (Admin Users) & Phân quyền (Roles) (Nếu cần)

**Thiết kế UI/UX:**

1.  **Cấu trúc Layout Chung:**
    *   **Sidebar (Thanh bên):** Chứa các liên kết điều hướng chính (Dashboard, Products, Categories, Orders, Customers, Settings, v.v.).
    *   **Top Navbar (Thanh điều hướng trên cùng):** Chứa thông tin người dùng hiện tại, thông báo, nút đăng xuất.
    *   **Content Area (Vùng nội dung chính):** Hiển thị nội dung của trang được chọn.
    *   Sử dụng Shadcn UI components cho các yếu tố UI cơ bản.

2.  **Các Tuyến đường (Routes) và Trang chính (Sử dụng Next.js App Router):**
    *   `/admin/dashboard`: Trang tổng quan, hiển thị các số liệu thống kê chính, biểu đồ.
    *   `/admin/products`: Danh sách sản phẩm (tìm kiếm, lọc, phân trang).
    *   `/admin/products/new`: Form tạo sản phẩm mới.
    *   `/admin/products/[id]/edit`: Form chỉnh sửa sản phẩm.
    *   `/admin/categories`: Danh sách danh mục sản phẩm.
    *   `/admin/categories/new`: Form tạo danh mục mới.
    *   `/admin/categories/[id]/edit`: Form chỉnh sửa danh mục.
    *   `/admin/orders`: Danh sách đơn hàng (lọc theo trạng thái, ngày tháng, v.v.).
    *   `/admin/orders/[id]`: Chi tiết đơn hàng.
    *   `/admin/customers`: Danh sách khách hàng.
    *   `/admin/customers/[id]`: Chi tiết khách hàng (lịch sử mua hàng).
    *   `/admin/settings`: Cài đặt chung cho trang quản trị.
    *   `/login`: Trang đăng nhập cho quản trị viên.

3.  **Các Thành phần UI Chung (Shadcn UI):**
    *   `Table`: Hiển thị dữ liệu dạng bảng (danh sách sản phẩm, đơn hàng).
    *   `Input`, `Select`, `Textarea`, `Checkbox`, `RadioGroup`, `Switch`: Cho các form nhập liệu.
    *   `Button`: Cho các hành động (Thêm mới, Lưu, Xóa).
    *   `Dialog`: Cho các thông báo xác nhận, form chỉnh sửa nhanh.
    *   `DropdownMenu`, `NavigationMenu`: Cho menu và điều hướng.
    *   `Pagination`: Cho phân trang danh sách.
    *   `Card`: Hiển thị thông tin dạng thẻ (trong dashboard).
    *   `Tabs`: Để phân chia nội dung trong một trang.
    *   `DatePicker`: Chọn ngày tháng.
    *   `Tooltip`, `Popover`: Cung cấp thêm thông tin.

**Kiến trúc Component:**

*   **`AdminLayout`:** Component chính bao bọc toàn bộ layout admin (sidebar, navbar).
*   **Page Components:** Mỗi route sẽ có một component chính (ví dụ: `ProductsPage`, `OrdersPage`).
*   **Reusable Components:**
    *   `DataTableWrapper`: Component tùy biến dựa trên Shadcn `Table` để hiển thị, sắp xếp, lọc dữ liệu.
    *   `EntityForm`: Component form chung có thể tái sử dụng cho việc tạo/sửa các thực thể (ProductForm, CategoryForm).
    *   `StatCard`: Component thẻ hiển thị số liệu trên dashboard.
    *   `SearchInput`, `FilterDropdowns`: Các component cho chức năng tìm kiếm và lọc.
*   **Server Components và Client Components (Next.js):**
    *   Sử dụng Server Components cho các trang/component chủ yếu hiển thị dữ liệu tĩnh hoặc dữ liệu được fetch từ server.
    *   Sử dụng Client Components (`"use client"`) cho các trang/component có tương tác người dùng, sử dụng state, effects, hoặc các browser API.

**Quản lý Trạng thái (State Management) và Luồng Dữ liệu:**

1.  **Fetching Dữ liệu từ các API E-commerce (thông tin API qua MCP):**
    *   Tạo một lớp service/các custom hooks (ví dụ: `useProductApi`, `useOrderApi`) để trừu tượng hóa việc gọi API.
    *   Sử dụng React Query (hoặc SWR) để quản lý server state: caching, optimistic updates, background refetching, error handling cho API requests.
2.  **Trạng thái Phía Client:**
    *   **Local Component State (`useState`, `useReducer`):** Cho các trạng thái UI cục bộ (ví dụ: trạng thái mở/đóng của modal, giá trị input trong form chưa submit).
    *   **Global State (Zustand hoặc React Context API):** Cho các trạng thái cần chia sẻ toàn cục như thông tin người dùng đăng nhập, theme (sáng/tối), cấu hình chung.
3.  **Forms:**
    *   Sử dụng `react-hook-form` cho quản lý form.
    *   Sử dụng `zod` để định nghĩa schema và validation cho form.

**Tương tác với các API E-commerce (chi tiết API lấy từ MCP):**
*   **Xác thực (Authentication API - chi tiết lấy từ MCP):**
    *   Ví dụ: `POST /api/auth/login`: Để đăng nhập, trả về JWT.
    *   Ví dụ: `GET /api/auth/me`: Lấy thông tin người dùng hiện tại.
    *   Header `Authorization: Bearer <token>` sẽ được gửi kèm mỗi request tới API được bảo vệ.
*   **Products API (chi tiết lấy từ MCP - Ví dụ):**
    *   `GET /api/products?search=...&category=...&page=...`
    *   `POST /api/products` (body: product data)
    *   `GET /api/products/{id}`
    *   `PUT /api/products/{id}` (body: product data)
    *   `DELETE /api/products/{id}`
*   **(Tương tự cho Categories API, Orders API, Customers API - chi tiết lấy từ MCP)**
*   **Quan trọng:** Các endpoints trên chỉ là ví dụ. Cần tham chiếu thông tin chi tiết về các API này qua MCP. Nếu MCP cung cấp file OpenAPI spec cho "DecorStore API" (hoặc API E-commerce tương ứng), chúng ta có thể sử dụng các tool như `mcp_API_specification_read_project_oas_4ffjm1` để đọc và hiểu rõ hơn về các endpoints và cấu trúc dữ liệu.

**Công nghệ Sử dụng:**

*   **Next.js 13+ (App Router)**
*   **React 18+**
*   **TypeScript**
*   **Tailwind CSS**
*   **Shadcn UI** (và Radix UI primitives)
*   **React Query** (hoặc SWR) cho server state.
*   **Zustand** (hoặc Context API) cho global client state.
*   **`react-hook-form`** và **`zod`** cho quản lý và validation form.
*   **ESLint, Prettier** cho code quality.
*   **Jest, React Testing Library** cho testing.

**Khả năng Tiếp cận (Accessibility - a11y):**

*   Sử dụng HTML ngữ nghĩa.
*   Đảm bảo tất cả các thành phần tương tác có thể điều khiển bằng bàn phím.
*   Sử dụng ARIA attributes khi cần thiết (Shadcn UI đã hỗ trợ tốt phần này).
*   Kiểm tra độ tương phản màu sắc.

**Xử lý Lỗi (Error Handling):**

*   Hiển thị thông báo lỗi thân thiện cho người dùng khi API request thất bại (sử dụng Shadcn `Toast` hoặc `Alert`).
*   Validation messages tại các trường trong form.
*   Trang 404 cho các route không tồn tại.
*   Chuyển hướng về trang login nếu người dùng chưa xác thực hoặc token hết hạn.

### Danh sách Công việc Chi tiết (Task Breakdown)

Dưới đây là các bước chi tiết để khởi tạo và xây dựng dự án:

- [ ] **Giai đoạn 1: Thiết lập Dự án và Cấu hình Cơ bản**
    - [x] **Khởi tạo Dự án Next.js:**        - [x] Chạy `npx create-next-app@latest next-admin --typescript --tailwind --eslint`        - [x] Cấu hình `tsconfig.json` và `tailwind.config.js` theo chuẩn dự án.
        - [x] **Cài đặt Shadcn UI:**        - [x] Chạy `npx shadcn@latest init` và cấu hình theo hướng dẫn.        - [x] Cài đặt các components cơ bản ban đầu (ví dụ: `button`, `input`, `table`, `dialog`).
        - [ ] **Cấu trúc Thư mục Dự án:**        - [ ] Tạo cấu trúc thư mục chuẩn cho `app`, `components`, `lib`, `hooks`, `services`, `styles`, `types`, etc.
        *   Ví dụ:
            ```
            /app
                /(admin) # Group cho các route admin
                    /dashboard
                    /products
                    /categories
                    /orders
                    /customers
                    /settings
                    /layout.tsx # Layout chung cho admin
                /login
                /layout.tsx # Layout gốc
            /components
                /ui # Shadcn UI components
                /shared # Các components tái sử dụng
                /layouts # Components layout (AdminLayout)
            /lib # Utilities, helpers
            /hooks # Custom React hooks
            /services # API interaction logic
            /types # TypeScript type definitions
            ```
        - [ ] **Cài đặt Thư viện Quản lý Trạng thái:**        - [ ] Cài đặt `react-query` (hoặc SWR) và cấu hình `QueryClientProvider`.        - [ ] Cài đặt `zustand` (nếu quyết định sử dụng cho global state).
        - [ ] **Cài đặt Thư viện Form:**        - [ ] Cài đặt `react-hook-form` và `zod`.
        - [ ] **Cấu hình ESLint và Prettier:**        - [ ] Đảm bảo rules được áp dụng nhất quán.        - [ ] Thêm script `lint` và `format` vào `package.json`.
        - [ ] **Thiết lập Biến Môi trường:**        - [ ] Tạo file `.env.local` để lưu trữ URL của các API E-commerce (thông tin lấy từ MCP) và các thông tin nhạy cảm khác. (Ví dụ: `NEXT_PUBLIC_PRODUCTS_API_URL=...`, `NEXT_PUBLIC_AUTH_API_URL=...`)

- [ ] **Giai đoạn 2: Xây dựng Layout Chính và Xác thực**
    - [ ] **Component `AdminLayout`:**
        - [ ] Tạo component `AdminLayout` (`/components/layouts/AdminLayout.tsx`).
        - [ ] Implement Sidebar với các mục điều hướng cơ bản (sử dụng Shadcn `NavigationMenu` hoặc custom).
        - [ ] Implement Top Navbar hiển thị logo, thông tin người dùng (placeholder), nút đăng xuất.
        - [ ] Áp dụng `AdminLayout` cho các route trong group `(admin)` tại `/app/(admin)/layout.tsx`.
    - [ ] **Trang Đăng nhập (`/login`):**
        - [ ] Tạo UI cho trang đăng nhập (`/app/login/page.tsx`).
        - [ ] Implement form đăng nhập sử dụng Shadcn `Input`, `Button`, `react-hook-form`, `zod`.
        - [ ] Style form bằng Tailwind CSS.
    - [ ] **Tích hợp API Đăng nhập (chi tiết API lấy từ MCP):**
        - [ ] Tạo service function để gọi API đăng nhập (ví dụ: `POST /api/Auth/login`).
        - [ ] Xử lý lưu trữ JWT token (ví dụ: trong `localStorage` hoặc `httpOnly cookie`).
        - [ ] Xử lý trạng thái loading, success, error.
        - [ ] Chuyển hướng người dùng đến `/dashboard` sau khi đăng nhập thành công.
    - [ ] **Bảo vệ Routes Admin:**
        - [ ] Implement middleware trong Next.js (`middleware.ts`) để kiểm tra authentication token.
        - [ ] Nếu chưa đăng nhập, redirect về `/login`.
    - [ ] **Chức năng Đăng xuất:**
        - [ ] Implement nút đăng xuất trên Sidebar.
        - [ ] Xóa token và redirect về `/login`.
    - [ ] **Hiển thị Thông tin Người dùng (chi tiết API lấy từ MCP):**
        - [ ] Tạo service function để gọi API lấy thông tin người dùng (ví dụ: `GET /api/Auth/user`).
        - [ ] Lưu thông tin người dùng vào global state (Zustand/Context).
        - [ ] Hiển thị tên người dùng trên Navbar.

- [ ] **Giai đoạn 3: Xây dựng Module Quản lý (Ví dụ: Sản phẩm - Lặp lại cho các module khác)**
    - [ ] **Đọc và Hiểu Đặc tả API (thông qua MCP):**
        - [ ] **(Ưu tiên)** Sử dụng MCP để tìm và hiểu đặc tả của các API E-commerce cần thiết (Products API, Categories API, Orders API, Customers API).
        - [ ] Nếu MCP cung cấp file OpenAPI spec (ví dụ: cho "DecorStore API" hoặc API tương đương), sử dụng tool để đọc spec:
            - [ ] Gọi `mcp_API_specification_read_project_oas_k5k05p()` để lấy nội dung spec nếu nó liên quan đến "DecorStore API" hoặc API E-commerce chính.
            - [ ] Nếu có `$ref`, gọi `mcp_API_specification_read_project_oas_ref_resources_k5k05p()` để lấy chi tiết.
        - [ ] Nghiên cứu kỹ tài liệu API (fields, query params, request/response formats) cho từng thực thể.
    - [ ] **Module Sản phẩm - Trang Danh sách (`/admin/products`):**
        - [ ] Tạo trang `/app/(admin)/products/page.tsx`.
        - [ ] **Component `ProductTable` (Tái sử dụng):**
            - [ ] Thiết kế component `ProductTable` sử dụng Shadcn `Table`, `TableHeader`, `TableBody`, `TableRow`, `TableHead`, `TableCell`.
            - [ ] Định nghĩa các cột: Ảnh, Tên, SKU, Danh mục, Giá, Tồn kho, Trạng thái, Hành động (Sửa, Xóa).
            - [ ] Style table và cells bằng Tailwind CSS.
        - [ ] **Tích hợp API Lấy Danh sách Sản phẩm (chi tiết API lấy từ MCP):**
            - [ ] Tạo service function/hook `useGetProducts` để gọi API danh sách sản phẩm (ví dụ: `GET /api/products`).
            - [ ] Sử dụng `react-query` để fetch và cache dữ liệu.
            - [ ] Xử lý loading, error states.
        - [ ] **Hiển thị Dữ liệu:**
            - [ ] Truyền dữ liệu sản phẩm vào `ProductTable`.
        - [ ] **Chức năng Tìm kiếm:**
            - [ ] Thêm Shadcn `Input` cho tìm kiếm.
            - [ ] Cập nhật state tìm kiếm và gọi lại API (có thể debounce).
        - [ ] **Chức năng Lọc:**
            - [ ] Thêm Shadcn `Select` hoặc `DropdownMenu` để lọc theo Danh mục, Trạng thái.
            - [ ] Cập nhật state bộ lọc và gọi lại API.
        - [ ] **Chức năng Phân trang:**
            - [ ] Implement Shadcn `Pagination`.
            - [ ] Cập nhật state trang hiện tại và gọi lại API.
        - [ ] **Nút "Thêm Sản phẩm":**
            - [ ] Thêm Shadcn `Button` điều hướng đến `/admin/products/new`.
        - [ ] **Hành động trên dòng (Sửa/Xóa):**
            - [ ] Thêm cột "Hành động" với nút Sửa (link đến edit page) và nút Xóa.

    - [ ] **Module Sản phẩm - Trang Tạo/Sửa (`/admin/products/new`, `/admin/products/[id]/edit`):**
        - [ ] Tạo trang `/app/(admin)/products/new/page.tsx` và `/app/(admin)/products/[id]/edit/page.tsx`.
        - [ ] **Component `ProductForm` (Tái sử dụng):**
            - [ ] Thiết kế component form với các trường: Tên, Mô tả (Rich Text Editor nếu cần), Giá, SKU, Tồn kho, Danh mục (Select), Ảnh sản phẩm (Upload), Trạng thái (Select/Switch), v.v.
            - [ ] Sử dụng Shadcn UI components cho các trường form.
            - [ ] Sử dụng `react-hook-form` và `zod` cho validation.
        - [ ] **Tích hợp API Tạo/Cập nhật Sản phẩm (chi tiết API lấy từ MCP):**
            - [ ] Tạo service functions/hooks `useCreateProduct`, `useUpdateProduct`.
            - [ ] Gọi API tạo (ví dụ: `POST /api/products`) hoặc cập nhật sản phẩm (ví dụ: `PUT /api/products/{id}`).
            - [ ] Xử lý upload ảnh (nếu API hỗ trợ).
            - [ ] Hiển thị thông báo thành công/thất bại (Shadcn `Toast`).
            - [ ] Redirect về trang danh sách sản phẩm sau khi thành công.
        - [ ] **Tải Dữ liệu Sản phẩm (cho trang Edit, chi tiết API lấy từ MCP):**
            - [ ] Tạo service function/hook `useGetProductById` để gọi API lấy chi tiết sản phẩm (ví dụ: `GET /api/products/{id}`).
            - [ ] Điền dữ liệu vào form.

    - [ ] **Module Sản phẩm - Chức năng Xóa (chi tiết API lấy từ MCP):**
        - [ ] Implement logic xóa sản phẩm khi click nút Xóa trên `ProductTable`.
        - [ ] Hiển thị Shadcn `Dialog` để xác nhận trước khi xóa.
        - [ ] Gọi API xóa sản phẩm (ví dụ: `DELETE /api/products/{id}`).
        - [ ] Cập nhật lại danh sách sản phẩm (refresh data từ `react-query`).
        - [ ] Hiển thị thông báo thành công/thất bại.

- [ ] **Giai đoạn 4: Xây dựng các Module Quản lý Khác (Tương tự Module Sản phẩm, chi tiết API lấy từ MCP)**
    - [ ] **Module Danh mục Sản phẩm (`/admin/categories`):**
        - [ ] List, Create, Edit, Delete forms và API integration.
    - [ ] **Module Đơn hàng (`/admin/orders`):**
        - [ ] **Trang Danh sách Đơn hàng:**
            - [ ] Table hiển thị: Mã ĐH, Khách hàng, Ngày đặt, Tổng tiền, Trạng thái.
            - [ ] Lọc theo trạng thái, ngày, khách hàng.
            - [ ] Link xem chi tiết đơn hàng.
        - [ ] **Trang Chi tiết Đơn hàng (`/admin/orders/[id]`):**
            - [ ] Hiển thị thông tin chi tiết: sản phẩm trong đơn, thông tin giao hàng, lịch sử cập nhật trạng thái.
            - [ ] Chức năng cập nhật trạng thái đơn hàng.
    - [ ] **Module Khách hàng (`/admin/customers`):**
        - [ ] **Trang Danh sách Khách hàng:**
            - [ ] Table hiển thị: Tên, Email, Điện thoại, Tổng số đơn hàng.
            - [ ] Tìm kiếm khách hàng.
        - [ ] **Trang Chi tiết Khách hàng (`/admin/customers/[id]` - Tùy chọn, có thể là modal):**
            - [ ] Hiển thị thông tin cá nhân, địa chỉ, lịch sử mua hàng.

- [ ] **Giai đoạn 5: Trang Dashboard và Cài đặt**
    - [ ] **Trang Dashboard (`/admin/dashboard`):**
        - [ ] Thiết kế UI hiển thị các số liệu thống kê quan trọng (ví dụ: Doanh thu, Số đơn hàng mới, Sản phẩm bán chạy) bằng Shadcn `Card`.
        - [ ] (Tùy chọn) Tích hợp biểu đồ (sử dụng `recharts` hoặc thư viện tương tự, bọc trong component React).
        - [ ] Fetch dữ liệu tổng hợp từ các API E-commerce (chi tiết API lấy từ MCP).
    - [ ] **Trang Cài đặt (`/admin/settings`):**
        - [ ] Implement các form cài đặt cơ bản (nếu có).

- [ ] **Giai đoạn 6: Hoàn thiện, Tối ưu và Kiểm thử**
    - [ ] **Responsiveness:**
        - [ ] Kiểm tra và đảm bảo giao diện responsive trên các thiết bị (desktop, tablet, mobile) sử dụng Tailwind CSS utility classes.
    - [ ] **Accessibility (a11y):**
        - [ ] Rà soát toàn bộ ứng dụng để đảm bảo tuân thủ các tiêu chuẩn accessibility.
        - [ ] Sử dụng các công cụ kiểm tra a11y.
    - [ ] **Xử lý Lỗi Toàn diện:**
        - [ ] Implement global error handler (ví dụ: hiển thị `Toast` cho lỗi API không mong muốn).
        - [ ] Đảm bảo các thông báo lỗi rõ ràng, thân thiện.
    - [ ] **Tối ưu Hiệu năng:**
        - [ ] Rà soát các component, memoization (`React.memo`) nếu cần.
        - [ ] Tối ưu API calls.
        - [ ] Sử dụng Next.js dynamic imports cho code splitting nếu cần.
    - [ ] **Viết Unit Tests và Integration Tests:**
        - [ ] Unit test cho các custom hooks, utility functions, components phức tạp (sử dụng Jest, React Testing Library).
        - [ ] Integration test cho các luồng người dùng chính (đăng nhập, tạo sản phẩm, xem đơn hàng).
    - [ ] **Documentation:**
        - [ ] Viết JSDoc/TSDoc cho các components, props, functions quan trọng.
        - [ ] Cập nhật `README.md` với hướng dẫn cài đặt, chạy dự án, và các thông tin cần thiết khác.
    - [ ] **Dọn dẹp Code và Refactor:**
        - [ ] Review code, loại bỏ code thừa, refactor để cải thiện readability và maintainability.