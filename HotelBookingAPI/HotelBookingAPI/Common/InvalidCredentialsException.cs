namespace HotelBookingAPI.Common;

// Ném khi sai tên đăng nhập hoặc mật khẩu, middleware đổi thành 401
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string message) : base(message)
    {
    }
}