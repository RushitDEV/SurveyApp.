using AutoMapper;
using SurveyApp.Models;
using SurveyApp.ViewModels;
using System;

namespace SurveyApp.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Survey Mappings
            CreateMap<Survey, SurveyListViewModel>()
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses.Count));

            CreateMap<Survey, SurveyDetailViewModel>()
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses.Count));

            CreateMap<Survey, SurveyTakeViewModel>();

            CreateMap<Survey, SurveyStatisticsViewModel>()
                .ForMember(dest => dest.TotalResponses, opt => opt.MapFrom(src => src.Responses.Count));

            CreateMap<SurveyCreateEditViewModel, Survey>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore())
                .ForMember(dest => dest.Responses, opt => opt.Ignore());

            // Question Mappings
            CreateMap<Question, QuestionDetailViewModel>();
            CreateMap<Question, QuestionTakeViewModel>();
            CreateMap<Question, QuestionStatisticsViewModel>()
                .ForMember(dest => dest.TotalAnswers, opt => opt.MapFrom(src => src.Answers.Count));

            CreateMap<QuestionCreateEditViewModel, Question>()
                .ForMember(dest => dest.Survey, opt => opt.Ignore())
                .ForMember(dest => dest.SurveyId, opt => opt.Ignore())
                .ForMember(dest => dest.Options, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore())
                // 🔧 Enum dönüştürme (string → enum)
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertQuestionType(src.Type)));


            // Option Mappings
            CreateMap<Option, OptionViewModel>();
            CreateMap<Option, OptionStatisticsViewModel>()
                .ForMember(dest => dest.Count, opt => opt.Ignore())
                .ForMember(dest => dest.Percentage, opt => opt.Ignore());

            // Response Mappings
            CreateMap<Response, ResponseListViewModel>()
                .ForMember(dest => dest.AnswerCount, opt => opt.MapFrom(src => src.Answers.Count));

            CreateMap<Response, ResponseDetailViewModel>()
                .ForMember(dest => dest.SurveyTitle, opt => opt.MapFrom(src => src.Survey.Title));

            CreateMap<SubmitResponseViewModel, Response>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Survey, opt => opt.Ignore())
                .ForMember(dest => dest.Answers, opt => opt.Ignore())
                .ForMember(dest => dest.UserIp, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore());

            // Answer Mappings
            CreateMap<Answer, AnswerDetailViewModel>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                .ForMember(dest => dest.OptionText, opt => opt.MapFrom(src => src.Option != null ? src.Option.OptionText : null));

            CreateMap<AnswerSubmitViewModel, Answer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Response, opt => opt.Ignore())
                .ForMember(dest => dest.Question, opt => opt.Ignore())
                .ForMember(dest => dest.Option, opt => opt.Ignore())
                .ForMember(dest => dest.ResponseId, opt => opt.Ignore())
                .ForMember(dest => dest.AnswerDate, opt => opt.Ignore());
        }
        private static QuestionType ConvertQuestionType(string? type)
        {
            if (Enum.TryParse<QuestionType>(type, true, out var parsed))
                return parsed;
            return QuestionType.SingleChoice;
        }

    }

}
