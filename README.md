# DoNet

A Windows desktop app built on **WinUI 3 / Windows App SDK** (.NET 8).

The first screen is the sign-in page: a teal starfield backdrop, a frosted glass
card, and a custom title bar.

---

## Running it

Open `DoNet.sln` in Visual Studio 2022 (workload: *.NET Desktop Development* +
*Windows App SDK*), pick the `x64` platform, and press **F5**.

The app is wired to a demo identity provider, so sign-in works immediately:

| Field    | Accepts                                                |
| -------- | ------------------------------------------------------ |
| Username | any well-formed e-mail, e.g. `ada@donet.dev`           |
| Password | any 6+ characters — the literal `wrong` always fails   |

`Forgot Password?` and `Guest` are both live and produce a dialog.

---

## Project layout

```
DoNet/
├── App.xaml(.cs)              Startup; registers services, merges theme dictionaries
├── MainWindow.xaml(.cs)       Window chrome: custom title bar, sizing, hosts the frame
├── Views/
│   └── LoginPage.xaml(.cs)    The sign-in screen (layout + input plumbing only)
├── ViewModels/
│   └── LoginViewModel.cs      All sign-in state and behaviour; no XAML types
├── Services/
│   ├── IAuthenticationService.cs   Auth contract + session/result records
│   ├── DemoAuthenticationService.cs Stand-in provider (replace with the real one)
│   ├── ISystemStatusService.cs     Footer health indicator
│   └── AppServices.cs              Tiny service locator
├── Mvvm/                      ObservableObject, RelayCommand, AsyncRelayCommand
├── Converters/                Bool → Visibility / PasswordRevealMode
├── Themes/
│   ├── Theme.xaml             Design tokens: palette, typography, metrics
│   └── Controls.xaml          Control templates (button, links, inputs)
└── Assets/
    ├── Fonts/                 Outfit typeface (SIL OFL)
    └── Backgrounds/           Generated starfield PNG
```

### Plugging in a real backend

Everything the screen needs sits behind `IAuthenticationService`. Implement it
against your identity provider and change one line in
`Services/AppServices.RegisterDefaults()`:

```csharp
Register<IAuthenticationService>(new YourRealAuthService(...));
```

No view or view-model code has to change.

---

## Design notes

**Typography** — [Outfit](https://github.com/Outfitio/Outfit-Fonts) is embedded
in `Assets/Fonts` (Regular / Medium / SemiBold / Bold) and licensed under the SIL
Open Font License; see `Assets/Fonts/OFL.txt`. Font families are referenced per
weight because the static Outfit files declare distinct family names
(`Outfit`, `Outfit Medium`, `Outfit SemiBold`).

**Background** — `Assets/Backgrounds/login-background.png` is generated, not
hand-painted. Re-create or tweak it with:

```bash
pip install pillow
python tools/generate_background.py
```

The script is deterministic (fixed RNG seed), so the same stars come out every
time. Glow positions, colours and star density are constants at the top of the
file.

**Title bar** — `MainWindow` extends content into the caption area and themes the
system caption buttons transparent, so the starfield runs edge to edge. The
system buttons are kept (rather than hand-drawn) so Snap Layouts, the window
menu and accessibility behaviour all keep working; the left side carries the
custom brand lockup and session pill.

**Field chrome** — WinUI's stock `TextBox`/`PasswordBox` paint their own
background, border and focus underline. Those layers are neutralised in
`Themes/Theme.xaml` (the `TextControl*` keys) so the surrounding `Border` can
draw the field exactly as designed; focus is reflected onto that border from
code-behind.

**Passwords** — `PasswordBox.Password` is deliberately *not* data-bound. The view
forwards it to the view model on `PasswordChanged` so the secret never sits in
the binding engine.
