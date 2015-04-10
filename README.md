# Recaptcha2
ASP.NET MVC 5 widget for [google's recaptcha](https://www.google.com/recaptcha)

# Usage

1) Register in [google's recaptcha 2](https://www.google.com/recaptcha) and get site key and secret.

2) Add site key and secret into web.config:
```xml    
<configuration>
  <appSettings>
    <add key="Recaptcha2SiteKey" value="{Put here site key}"/>
    <add key="Recaptcha2Secret" value="{Put here secret}"/>
    ...
```

3) Add Recaptcha2 class to your ASP.NET MVC 5 project.

4) Add Recaptcha2 to your forms view:

```Razor
@using Recaptcha2
...
@using (Html.BeginForm()) 
{
...
        <div class="form-group">
            @Html.Label("Code", htmlAttributes: new { @class = "control-label col-md-2" })
            <div class="col-md-10">
                @Html.Recaptcha2()
                @Html.ValidationMessage("g-recaptcha-response", "", new { @class = "text-danger" })
            </div>
        </div>

```

   *No need to create a property in model for captcha code.

5) Add ValidateRecaptcha2 attribute to your controller's post action:

```C#
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateRecaptcha2(ErrorMessage = @"Recaptcha is required")]
```

Have fun.