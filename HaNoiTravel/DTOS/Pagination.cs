namespace HaNoiTravel.DTOS
{
    public class Pagination<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>(); // Danh sách các mục của trang hiện tại
        public int TotalCount { get; set; } // Tổng số mục trong toàn bộ kết quả (sau khi lọc)
        public int PageIndex { get; set; } // Index của trang hiện tại (thường bắt đầu từ 0)
        public int PageSize { get; set; }   // Số mục trên mỗi trang

        // Tính toán tổng số trang dựa trên TotalCount và PageSize
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        // Các thuộc tính giúp kiểm tra trang đầu/cuối (frontend có thể tự tính hoặc dùng cái này)
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1; // Nếu PageIndex bắt đầu từ 0
    }
}
