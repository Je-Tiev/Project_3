using AutoMapper;
using Paperless_Meeting.DTOs.Meeting;
using Paperless_Meeting.Models;

namespace Paperless_Meeting.Mappers
{
    public class MeetingMappingProfile : Profile
    {
        public MeetingMappingProfile()
        {
            // Map từ DTO tạo mới sang Model
            CreateMap<MeetingCreateDto, Models.Meeting>();

            // Map từ Model sang DTO hiển thị danh sách
            CreateMap<Models.Meeting, MeetingSummaryDto>();

            // Map từ Model sang DTO hiển thị chi tiết
            CreateMap<Models.Meeting, MeetingDetailDto>()
                // Ánh xạ đặc biệt cho danh sách người tham gia
                .ForMember(dest => dest.Participants,
                    opt => opt.MapFrom(src => src.Participants.Select(mp => mp.User)));
            
            // Map từ Model sang DTO hiển thị người dùng
            CreateMap<User, UserViewDto>();
        }
    }
}