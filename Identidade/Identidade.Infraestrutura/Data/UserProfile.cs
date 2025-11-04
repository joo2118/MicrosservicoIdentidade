using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Identidade.Dominio.Helpers;
using Identidade.Dominio.Interfaces;
using Identidade.Dominio.Modelos;
using Identidade.Publico.Dtos;

namespace Identidade.Infraestrutura.Data
{
    public class UserProfile : Profile
    {
        private readonly IReadOnlyRepository<User> _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;

        public UserProfile(IReadOnlyRepository<User> userRepository, IUserGroupRepository userGroupRepository)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;

            CreateMap<InputUserDto, User>()
                .ForMember(user => user.PasswordHash,
                    ex => ex.MapFrom(userDto => userDto.Password))
                .ForMember(user => user.LockoutEnd,
                    ex => ex.MapFrom(userDto => GetLockoutEnd(userDto)))
                .ForMember(user => user.UserGroupUsers,
                    ex => ex.MapFrom((userDto, originalUser) => GetUserGroupUsers(originalUser.UserName, userDto.UserGroups)))
                .ForMember(user => user.UserSubstitutions,
                    ex => ex.MapFrom((userDto, originalUser) => GetUserSubstitutions(originalUser.UserName, userDto.SubstituteUsers)))
                .ForMember(user => user.UserName,
                    ex => ex.MapFrom(userDto => userDto.Login))
                .ForAllMembers(option => option.Condition((source, destination, sourceMember) => sourceMember != null));

            CreateMap<ArcUserDto, User>()
                .ForMember(user => user.PasswordHash,
                    ex => ex.MapFrom(userDto => userDto.Password))
                .ForMember(user => user.LockoutEnd,
                    ex => ex.MapFrom(userDto => GetLockoutEnd(userDto)))
                .ForMember(user => user.UserGroupUsers,
                    ex => ex.MapFrom((userDto, originalUser) => GetUserGroupUsers(originalUser.UserName, userDto.UserGroups)))
                .ForMember(user => user.UserSubstitutions,
                    ex => ex.MapFrom((userDto, originalUser) => GetUserSubstitutions(originalUser.UserName, userDto.SubstituteUsers)))
                .ForMember(user => user.UserName,
                    ex => ex.MapFrom(userDto => userDto.Login))
                .ForAllMembers(option => option.Condition((source, destination, sourceMember) => sourceMember != null));

            CreateMap<ArcUserDto, ArcUser>()
                .ConvertUsing(user => new ArcUser(
                    user.Login,
                    user.Name,
                    user.Email,
                    user.Password,
                    user.Language.ToString(),
                    user.UserGroups,
                    user.SubstituteUsers,
                    user.AuthenticationType.ToString(),
                    user.PasswordExpiration,
                    user.PasswordDoesNotExpire ?? false,
                    user.Active ?? false));

            CreateMap<User, OutputUserDto>()
                .ForMember(userDto => userDto.Login,
                    ex => ex.MapFrom(user => user.UserName))
                .ForMember(userDto => userDto.Email,
                    ex => ex.MapFrom(user => user.Email))
                .ForMember(userDto => userDto.Active,
                    ex => ex.MapFrom(user => user.LockoutEnd == DateTimeOffset.MinValue))
                .ForMember(userDto => userDto.UserGroups,
                    ex => ex.MapFrom(user => user.UserGroupUsers.Select(ugu => ugu.UserGroupId).ToArray()))
                .ForMember(userDto => userDto.SubstituteUsers,
                    ex => ex.MapFrom(user => user.UserSubstitutions.Select(substituteUser => substituteUser.SubstituteUserId).ToArray()));
        }

        private DateTimeOffset? GetLockoutEnd(UserBaseDto userDto)
        {
            if (!(userDto.Active is bool activeNotNull))
                return null;

            return activeNotNull ? DateTimeOffset.MinValue : DateTimeOffset.MaxValue;
        }

        private ICollection<UserGroupUser> GetUserGroupUsers(string originalUserLogin, string[] userGroupIds)
        {
            if(userGroupIds is null)
                return Array.Empty<UserGroupUser>();

            return GetAndValidateUserGroups(originalUserLogin, userGroupIds).ToList();
        }

        private IEnumerable<UserGroupUser> GetAndValidateUserGroups(string originalUserLogin, string[] userGroupIds)
        {
            var user = ExceptionCatcher.ExecuteSafe<NotFoundAppException, User>(
                () => _userRepository.GetByName(originalUserLogin).GetAwaiter().GetResult(),
                e => { });

            if (user == null && !string.IsNullOrWhiteSpace(originalUserLogin))
                throw new NotFoundAppException("user", "login", originalUserLogin);

            var userGroups = _userGroupRepository.GetUserGroups(userGroupIds).GetAwaiter().GetResult();

            foreach (var group in userGroups)
            {
                yield return new UserGroupUser(group, user);
            }
        }

        private ICollection<UserSubstitution> GetUserSubstitutions(string originalUserLogin, string[] substituteUserIds)
        {
            if (substituteUserIds is null)
                return Array.Empty<UserSubstitution>();

            return GetAndValidateUserSubstitutions(originalUserLogin, substituteUserIds).ToList();
        }

        private IEnumerable<UserSubstitution> GetAndValidateUserSubstitutions(string originalUserLogin, string[] substituteUserIds)
        {
            var user = ExceptionCatcher.ExecuteSafe<NotFoundAppException, User>(
                () => _userRepository.GetByName(originalUserLogin).GetAwaiter().GetResult(),
                e => { });

            if (user == null && !string.IsNullOrWhiteSpace(originalUserLogin))
                throw new NotFoundAppException("user", "login", originalUserLogin);

            var substituteUsers = _userGroupRepository.GetUsers(substituteUserIds).GetAwaiter().GetResult();

            foreach (var substituteUser in substituteUsers)
            {
                yield return new UserSubstitution(user, substituteUser);
            }
        }
    }
}
