using Microsoft.AspNetCore.Http;
using Moq;

namespace RoomMate_Finder.Test.Helpers;

public static class HttpContextHelper
{
    public static IHttpContextAccessor CreateMockHttpContextAccessor(Guid currentUserId)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUserId"] = currentUserId;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return mockHttpContextAccessor.Object;
    }
}
