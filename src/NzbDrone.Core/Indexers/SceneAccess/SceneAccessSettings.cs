using System;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.SceneAccess
{
    public class SceneAccessSettingsValidator : AbstractValidator<SceneAccessSettings>
    {
        public SceneAccessSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.RssKey).NotEmpty();

            RuleFor(c => c.CookieUid).NotEmpty();
            RuleFor(c => c.CookieUid)
                .Matches(@"[0-9]{6}", RegexOptions.IgnoreCase)
                .WithMessage("Wrong pattern")
                .AsWarning();

            RuleFor(c => c.CookiePass).NotEmpty();
            RuleFor(c => c.CookiePass)
                .Matches(@"[0-9a-f]{32}", RegexOptions.IgnoreCase)
                .WithMessage("Wrong pattern")
                .AsWarning();
        }
    }

    public class SceneAccessSettings : IProviderConfig
    {
        private static readonly SceneAccessSettingsValidator validator = new SceneAccessSettingsValidator();

        public SceneAccessSettings()
        {
            BaseUrl = "https://sceneaccess.eu";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public String BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Cookie uid", HelpText = "SceneAccess uses a login cookie needed to access the search results, you'll have to retrieve it via a browser.")]
        public String CookieUid { get; set; }

        [FieldDefinition(2, Label = "Cookie pass", HelpText = "SceneAccess uses a login cookie needed to access the search results, you'll have to retrieve it via a browser.")]
        public String CookiePass { get; set; }

        [FieldDefinition(3, Label = "Rss Key")]
        public string RssKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(validator.Validate(this));
        }
    }
}
