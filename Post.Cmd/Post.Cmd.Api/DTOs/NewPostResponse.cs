using Post.Common.DTOs;
using System;

namespace Post.Cmd.Api.DTOs
{
    public class NewPostResponse : BaseResponse
    {
        public Guid Id { get; set; }
    }
}