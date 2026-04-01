# ⚡ Windows FPS Optimizer Pro v3.0 — C# WPF
## Ghid Compilare Visual Studio

---

## 📋 Cerinte

| Cerinta | Versiune |
|---------|----------|
| Visual Studio | 2022 (orice editie, inclusiv Community — **gratuit**) |
| .NET SDK | 8.0 (instalat automat cu VS) |
| Workload VS | **.NET desktop development** (bifat la instalare VS) |
| OS tinta | Windows 10 / 11 x64 |

---

## 🚀 Compilare rapida (recomandat)

### Varianta A — Visual Studio GUI

1. Deschide **Visual Studio 2022**
2. `File → Open → Project/Solution`
3. Selecteaza **`FPSOptimizer.csproj`**
4. Meniu sus: schimba din `Debug` → **`Release`**
5. `Build → Publish FPSOptimizer`
6. Selecteaza profil **"FolderProfile"** sau creeaza unul nou:
   - Target Runtime: `win-x64`
   - Deployment mode: `Self-contained`
   - File publish options: ✓ **Produce single file**
7. Click **Publish**
8. EXE-ul apare in `bin\Release\net8.0-windows\win-x64\publish\`

### Varianta B — Command Line (mai rapid)

Deschide **Developer PowerShell** sau **CMD** din VS (`Tools → Command Line`):

```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

EXE-ul final: `bin\Release\net8.0-windows\win-x64\publish\WindowsFPSOptimizer.exe`

**Dimensiune estimata: ~8-12 MB** (self-contained, fara nicio dependenta)

---

## 📁 Structura Proiect

```
FPSOptimizerCS/
├── FPSOptimizer.csproj          ← Proiect principal
├── app.manifest                 ← Cere drepturi Administrator
├── App.xaml / App.xaml.cs       ← Entry point aplicatie
│
├── Core/
│   ├── SystemInfo.cs            ← Model + Scanner hardware
│   ├── OptimizationOptions.cs   ← Module + definitii risc
│   └── OptimizerEngine.cs       ← Motor optimizare (toate modulele)
│
└── UI/
    ├── Themes.xaml              ← Stiluri dark gaming (culori, butoane, etc)
    ├── MainWindow.xaml/cs       ← Fereastra principala + navigare wizard
    └── Pages/
        ├── PageWelcome.xaml/cs  ← Pas 1: Introducere
        ├── PageScan.xaml/cs     ← Pas 2: Scanare hardware + sfaturi GPU
        ├── PageBackup.xaml/cs   ← Pas 3: Restore Point + backup registri
        ├── PageOptions.xaml/cs  ← Pas 4: Selectie module cu risc
        ├── PageExecute.xaml/cs  ← Pas 5: Executie live cu log colorat
        └── PageDone.xaml/cs     ← Pas 6: Finalizare + pasi urmatori
```

---

## ⚙️ Setari Publish recomandate (pentru .exe final)

In `FPSOptimizer.csproj` sunt deja configurate:

```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
```

Rezultat: **un singur .exe** care ruleaza pe orice Windows 10/11 x64 fara instalare .NET.

---

## 🔧 Troubleshooting

| Problema | Solutie |
|----------|---------|
| `The type 'Page' could not be found` | Instaleaza workload **.NET desktop development** in VS Installer |
| `CS0246: System.Management not found` | Proiectul referencieaza `System.Management` automat prin SDK — rebuild |
| Eroare manifest | Click dreapta proiect → Properties → Application → manifiest = `app.manifest` |
| EXE nu cere admin | Verifica ca `app.manifest` are `requireAdministrator` |

---

## 🛡️ Nota securitate

- Programul cere **UAC prompt** la fiecare pornire (necesar pentru modificari sistem)
- Antivirus poate detecta fals pozitiv la primul run (semneaza EXE-ul cu un certificat daca distribui)
- Recomandat: testeaza mai intai pe o masina virtuala sau cu toate optiunile de backup bifate

---

*Testat pe Windows 10 Build 19041+ si Windows 11 Build 22000+*
