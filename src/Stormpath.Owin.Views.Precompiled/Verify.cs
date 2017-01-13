namespace Stormpath.Owin.Views.Precompiled
{
    using System.Threading.Tasks;

    public class Verify : BaseView<Stormpath.Owin.Abstractions.ViewModel.VerifyEmailFormViewModel>
    {
        #line hidden
        public Verify()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            WriteLiteral(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>Log In</title>
    <meta content=""Didn't receive your verification email? No worries!"" name=""description"">
    <meta content=""width=device-width"" name=""viewport"">
    <style>
        html, body {
            height: 100%;
        }

        body {
            margin: 0;
        }

        .container {
            height: 100%;
            padding: 0;
            margin: 0;
            display: -webkit-box;
            display: -moz-box;
            display: -ms-flexbox;
            display: -webkit-flex;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .view {
            border: 1px solid rgb(183, 194, 205);
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""view verify-email-view""></div>
    </div>

    <script language=""javascript"">
        window.onload = function onDocumentReady() {
    ");
            WriteLiteral(@"        var stormpath = window.stormpath = new Stormpath({
                container: document.getElementsByClassName('verify-email-view')[0]
            });

            stormpath.showEmailVerification();
        };
    </script>
    <script language=""javascript"" src=""http://localhost:3000/js/app.js""></script>
</body>
</html>");
        }
        #pragma warning restore 1998
    }
}
