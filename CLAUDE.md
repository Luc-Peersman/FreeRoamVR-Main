# FreeRoamVR — projectcontext voor Claude

Laatst bijgewerkt: 2026-09-19. Dit bestand is bedoeld zodat een nieuwe Claude Code-sessie
(bv. na het hernoemen van deze projectmap) direct de relevante context heeft.

> Let op: dit bestand wordt niet automatisch onderhouden. Vraag Claude expliciet om het bij te
> werken nadat er wijzigingen zijn gedaan, anders raakt het verouderd.

## Wat is dit project

Aangepaste versie van Unity's officiële **VR Multiplayer Template** (namespace `XRMultiplayer`).
Social/mini-games VR-app: spelers met headsets zien elkaars avatars en spelen mini-games,
bedoeld om **volledig lokaal over LAN** te draaien (klaslokaal-scenario met meerdere headsets),
zonder afhankelijkheid van Unity Cloud-diensten.

- Unity-versie: 6000.5.4f1 (Unity 6)
- Networking: Netcode for GameObjects 2.13.0 + UnityTransport
- VR/XR: XR Interaction Toolkit 3.5.1, OpenXR 1.17.1, XR Hands 1.8.0 (Meta Quest gericht)
- Render pipeline: URP
- Player Settings: Company Name `SintLucas.IX`, Product Name `FreeRoamVR` (= naam die op de
  Quest-headset te zien is; geen custom AndroidManifest.xml die dit overschrijft)
- Hoofdscene: `Assets/Scenes/SampleScene.unity` (enige scene in `Assets/Scenes/`)

## LAN-specifieke aanpassingen (kern van dit project)

- `Assets/VRMPAssets/Scripts/Network/NetworkManagers/XRINetworkGameManager.cs`: eigen
  UDP-discoveryprotocol (broadcast poort 47777) waarmee clients automatisch een host op het
  LAN vinden/joinen. `CurrentSessionType` staat hard op `SessionType.LocalOnly`.
- `Assets/AutoRoomManager/AutoRoomManager.cs`: elke headset zoekt bij opstarten automatisch
  een host, joint of wordt zelf host, met reconnect-logica bij verbindingsverlies.
- **UGS-authenticatie en Vivox voice chat zijn NIET nodig en worden automatisch overgeslagen**
  zolang `SessionType.LocalOnly` actief blijft (zie `SessionManager.SetupLocalTransport()` en
  de guard-checks in `VoiceChatManager`). Dit is bewust zo gehouden: geen cloud-account nodig
  om avatars te laten synchroniseren op LAN.
- Er is **geen marker-based of anchor-based co-locatiesysteem** — spelers zien elkaar alleen
  via het netwerk, hun fysieke ruimtes zijn niet virtueel op elkaar uitgelijnd. De enige sporen
  hiervan zijn een uitgeschakelde (`m_enabled: 0`) Meta `ColocationDiscoveryFeature`-toggle in
  `Assets/XR/Settings/OpenXR Package Settings.asset` — niet actief gebruikt.

## Spawnsysteem (recent herschreven)

In `XRINetworkGameManager.cs` (rond regel 269-330):
- Spawnpositie/-rotatie komt volledig van een **verplicht** `m_FirstSpawnPoint` (Transform)-veld
  — géén hardcoded `Vector3` meer (die zijn bewust verwijderd op verzoek van de gebruiker).
- In `SampleScene.unity` is dit gekoppeld aan een los GameObject genaamd **"FirstSpawnPoint"**
  (root-level, geen kinderen).
- Volgende spelers spawnen op een lijn achter dit punt: `m_SpawnLineDirection` (default
  `(0,0,-1)`, dus lokale Z-as van het spawnpunt) × `m_SpawnSpacing` (nu **0.8 meter**, bewust
  gekozen omdat de standaard VR-veiligheidsrichtlijnen van ~2-3m tussen spelers in de praktijk
  van deze gebruiker niet haalbaar zijn — spelers worden fysiek ook op 0,8m van elkaar gezet).
- De rotatie van `m_FirstSpawnPoint` bepaalt de kijkrichting (diens lokale Z-as/blauwe gizmo-pijl).
- Als `m_FirstSpawnPoint` leeg is: spelers spawnen op `(0,0,0)` + logwaarschuwing.

Het GameObject "FirstSpawnPoint" stond ooit op `(0.54, 3.84, 3.19)`, zwevend boven de vloer.
**Correctie (bevestigd door gebruiker, 2026-09-18): staat nu op `(-1.41, 0, 3.78)`, rotatie
270° om Y — dit is de correcte, gewenste waarde**, op vloerhoogte (Y=0). Een eerdere
sessie-notitie hier claimde `(-2, 0, 5)` als de juiste/geverifieerde waarde, maar dat bleek
**stale/onjuist** — die fix is kennelijk nooit gecommit of is onderweg teruggedraaid; de
git-gecommitte scene had altijd al `(-1.41, 0, 3.78)` staan. **Les: vertrouw voor de exacte
huidige transform-waarden altijd de scene zelf (of live `[SpawnDebug]`-logoutput), niet enkel
een eerdere sessienotitie hier**, ook al staat er "geverifieerd" bij.

## Belangrijke technische feiten (voor advies aan de gebruiker)

- **Correctie (2026-09-16):** het rig-prefab dat hierboven stond genoemd
  (`XRMPT_XR_Origin_Setup.prefab`, met `m_RequestedTrackingOriginMode: 2` = Floor) blijkt **niet
  het prefab dat effectief in `SampleScene.unity` staat** — dat is vrijwel ongebruikt (enkel via
  een Tutorial-variant). Het werkelijk geplaatste rig is
  `Assets/VRMPAssets/Prefabs/PrefabVariants/XR Origin Hands (XR Rig) MP Template Variant.prefab`,
  dat via zijn basisprefab-keten (XRI Starter Assets `XR Origin (XR Rig).prefab`)
  `m_RequestedTrackingOriginMode: 0` (**NotSpecified**, dus géén Floor) en
  `m_CameraYOffset: 1.36144` erft. Zonder expliciet Floor aan te vragen kan XROrigin
  terugvallen op "Device"-tracking en die 1.36m offset alsnog optellen bij de camerahoogte —
  dit was (vermoedelijk) de oorzaak van het "vloer op navelhoogte"-probleem dat gerapporteerd
  werd. **Fix toegepast:** in `XRINetworkPlayer.cs` (`OnNetworkSpawn`, `IsLocalPlayer`-blok)
  wordt nu expliciet `m_XROrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;`
  gezet vóór het positioneren van de rig.
  ❌ Getest op 2026-09-16 en toen NIET opgelost bevonden (vloer op navelhoogte).
  **✅ Correctie (2026-09-18): dit klopte niet — live `[SpawnDebug]`-Logcat-metingen op de
  Quest 3 tonen `CurrentTrackingOriginMode=Floor` (dus de fix werkt écht) en een
  `cameraHeightAboveRig` van ~1,31m, wat gewoon een normale echte ooghoogte is, geen bug.**
  De 2026-09-16-conclusie was blijkbaar gebaseerd op een verkeerde/onvolledige test (zie ook
  de andere correctie in "Spawnsysteem" hierboven over stale sessienotities).
  **✅ Update (2026-09-18): ook het visuele vloer-mesh "Floor (1)" is getest en staat exact op
  wereld-Y=0** — dus alle drie de code-kanten (spawnpositie, Floor-tracking-mode,
  Floor-meshpositie) zijn nu bevestigd correct, zie TODO #5 voor details. De enige resterende
  verdachte voor een eventuele "vloer klopt niet"-perceptie is **Guardian/vloercalibratie op
  Q zelf** (headset-side, geen code-bug).
- Ooghoogte komt normaliter van de headset-tracking zelf (fysieke lengte van de speler) zodra
  Floor-mode effectief actief is, NIET van een handmatige Y-waarde. Zet spawnpunten dus op
  vloerhoogte, niet op een "geschatte ooghoogte".
- 1 meter in VR = 1 meter fysiek: XR Origin-rig heeft `m_LocalScale = (1,1,1)`, geen
  schaalfactor toegepast.
- Geen hardcoded absolute paden (`C:\Users\...`) gevonden in projectbestanden — een
  mapnaam-wijziging van dit project is dus veilig.
- `cloudProjectId`/`organizationId` in `ProjectSettings/ProjectSettings.asset` is gekoppeld aan
  het Unity Cloud-account van de eigenaar, maar heeft **geen runtime-effect** zolang
  `SessionType.LocalOnly` actief blijft (UGS-calls worden dan nooit aangeroepen).
  **Bijwerking (bevestigd 2026-09-18):** bij het builden (klassieke Build-knop, niet OVR Build
  APK) naar Quest 3 verschijnt een dialoogvenster **"Missing Project ID"** — "Because you are
  not a member of this project this build will not access Unity services. Do you want to
  continue?". Dit komt doordat de ingelogde Unity-editor-gebruiker geen lid is van het
  gekoppelde Unity Cloud-project (`cloudProjectId`), maar is **onschadelijk en verwacht**
  gezien `SessionType.LocalOnly` — gewoon op **"Yes"** klikken om door te bouwen; er wordt
  toch nooit een UGS-call gedaan.
  **Zelfde grondoorzaak, ander symptoom (bevestigd 2026-09-18):** bij het **opstarten van
  Unity** (los van builden) verschijnt soms een console-error `System.Exception: HTTP/1.1 403
  Forbidden` vanuit `Unity.Services.Vivox.Editor.VivoxApiClient` (`GetAndSetVivoxCredentials`).
  Dit is de Vivox-package die bij editor-opstart credentials probeert op te halen bij Unity
  Cloud — faalt om dezelfde reden (geen lid van het cloud-project). **Puur een Editor-tooling-
  aanroep, onschadelijk, geen effect op builds of runtime** (VoiceChatManager slaat Vivox
  sowieso al over via de `SessionType.LocalOnly`-guard). Negeren.
- Dit is **geen git-repository** (op het moment van schrijven).

## Werkomgeving: meerdere PC's

Gebruiker werkt aan dit project vanaf drie verschillende machines, in gesprekken aangeduid als:
- **PCIX** — de machine waarop de huidige projectstaat oorspronkelijk is opgebouwd (volledige
  voorgeschiedenis van de setup-fixes hieronder). Heeft de Meta XR-tooling
  (`com.meta.xr.sdk.all`, v205.0.0) al geïnstalleerd.
- **Omen** — sinds 2026-09-18 in gebruik; had de Meta XR-tooling nog niet geïnstalleerd, dit
  is die sessie gestart.
- **laptop** — derde machine, tooling-status nog niet in kaart gebracht.

**Let op:** machine-specifieke lokale staat (geïnstalleerde Unity-packages, `Library/PackageCache`-
patches zoals TODO #10, ADB/MQDH-koppeling met de headset) gaat niet mee via git — elke machine
heeft zijn eigen eenmalige lokale setup nodig, ook al zijn de projectbestanden gedeeld.

## Openstaande aandachtspunten / TODO's

1. "Gameplay"-container in de scene staat op inactief (`m_IsActive: 0`) — vermoedelijk bewust
   (geactiveerd via `ConnectionToggler.cs` bij verbinding), maar nog niet expliciet bevestigd.
   Status ongewijzigd, geverifieerd op 2026-09-16.
2. Er staat een verweesde scene-override `m_CheckForAutoReconnect` op het
   `VoiceChatManager`-component (verwijst naar een veld dat niet meer in de code bestaat) —
   functioneel onschadelijk. Nog steeds aanwezig na de laatste scene-save (2026-09-16), ruimt
   zichzelf blijkbaar niet vanzelf op; puur cosmetisch, geen actie vereist tenzij gewenst.
3. Fysieke veiligheidsafstand tussen spelers (0,8m) is een bewuste afwijking van de
   gebruikelijke VR-richtlijnen (2-3m) vanwege ruimtegebrek — houd hier rekening mee bij
   suggesties over mini-games die fysieke beweging vereisen (bv. Slingshot).
4. ✅ **Afgehandeld:** Scripting Backend (Android) stond tijdelijk op Mono, is teruggezet naar
   **IL2CPP** (`scriptingBackend: Android: 1` in `ProjectSettings/ProjectSettings.asset`,
   2026-09-16) — zie de sectie "Build performance" hieronder voor de afweging.
5. ✅ **Grotendeels opgehelderd (2026-09-18) via live `[SpawnDebug]`-Logcat-analyse op de
   Quest 3 (zie TODO #6):**
   - **Spawnpositie X/Z: GEEN bug.** `GetSpawnPositionForIndex(0)` retourneert exact
     `m_FirstSpawnPoint.position` (geen offset bij index 0) — de gelogde spawnPos
     `(-1.41, 0.00, 3.78)` kwam exact overeen met de daadwerkelijke scene-positie van
     "FirstSpawnPoint". De eerdere aanname dat dit punt op `(-2, 0, 5)` hoorde te staan was
     **stale documentatie** (zie "Spawnsysteem" hierboven) — **gebruiker bevestigde dat
     `(-1.41, 0, 3.78)` de correcte, gewenste waarde is.** Geen codewijziging nodig.
   - **Floor-tracking-fix werkt wél degelijk** (in tegenstelling tot de eerdere conclusie
     hieronder bij "Belangrijke technische feiten"): live log toont
     `CurrentTrackingOriginMode=Floor` (niet Device/NotSpecified), zowel direct bij spawn als
     op +0,5s/+2s. De rig-root staat correct op Y=0.
   - **`cameraHeightAboveRig` ≈ 1,31–1,33m is GEEN bug** — dat is een normale, echte
     staande ooghoogte. Dit getal groeit licht over tijd (1,314 → 1,326 na 2s), vermoedelijk
     gewoon natuurlijke hoofdbeweging, geen zorgwekkende drift.
   - `Floor (1)` wereldpositie logde exact `(0.00, 0.00, 0.00)` — de **transform** van het
     vloer-object zelf klopte dus altijd al.
   - ✅ **Definitief opgelost (2026-09-19) — echte oorzaak was iets heel anders dan een
     transform-bug:** gebruiker ontdekte fysiek op Q dat je bij bukken **onder de vloer-mesh
     door kon kijken**, met een correct geplaatst grid-object ("GridVisuals", onderdeel van
     `Level.prefab`) zichtbaar op de échte vloer eronder. Onderzoek van de scene-YAML wees uit:
     **"Floor (1)" bleek een los, later handmatig aan de scene toegevoegd GameObject** (staat
     als `AddedGameObject` in de prefab-modificaties van de Level-prefab-instantie, hoort dus
     niet bij het originele `Level.prefab`-asset). Het hergebruikte de ruwe submesh uit
     `Assets/VRMPAssets/Meshes/EnvironmentMeshes/MP_Level.fbx` met **Local Scale (1,1,1)** en
     **Local Position (0,0,0)** — puur om een collider + XRI **Teleport Area**-component te
     hebben voor teleport-locomotion. Maar diezelfde ruwe FBX-submesh wordt elders in
     `Level.prefab` correct gebruikt met **Local Scale (0.75, 0.75, 0.75)** en **Local Position
     Y = −0.26** (blijkbaar nodig om de export-schaal/pivot van de FBX te compenseren) — "Floor
     (1)" miste die correctie volledig, vandaar dat de *mesh* (niet zijn transform) veel te
     hoog en te groot in de lucht hing, ondanks dat `transform.position` netjes op (0,0,0) stond.
     Bij visueel uitlijnen bleek de benodigde Y-correctie voor déze specifieke plek in de
     hiërarchie zelfs −7,3 te zijn (niet −0,26 — de parent-schaalfactor verschilt van de plek
     waar de mesh elders in het prefab wordt gebruikt).
     **Gekozen fix (schoner dan een magische offset-waarde):** "Floor (1)" is **uitgezet/
     verwijderd** uit de scene. Er is een **nieuwe, simpele Plane** aangemaakt op de echte
     vloerhoogte (samenvallend met "GridVisuals"), met daarop een **Mesh Collider** en hetzelfde
     **Teleport Area-script** dat op "Floor (1)" zat, zodat teleport-locomotion behouden blijft.
     Geen enkel ander script bleek "Floor (1)" bij naam te gebruiken (enkel de eigen
     `[SpawnDebug]`-log in `XRINetworkPlayer.cs`, functioneel onschadelijk als het object weg is).
     **Bevestigd door gebruiker (2026-09-19): vloer is nu correct, geen "onder de vloer kunnen
     kijken" meer.** Guardian/vloercalibratie op Q was dus **niet** de oorzaak.
6. **Debug-logging is uitgebreid (2026-09-18), nieuwste ronde nog niet getest.** In
   `XRINetworkPlayer.cs` (`OnNetworkSpawn` + coroutine `LogSpawnPositionDelayed`), prefix
   `[SpawnDebug]`, logt nu: spawnIndex/spawnPos/spawnRot, rig-positie na het zetten (+0,5s/+2s),
   `TrackingOriginMode` (Requested+Current+CameraYOffset), `cameraHeightAboveRig`
   (camera.position.y − rig.position.y — dit ÍS het navelhoogte-symptoom als het echt een bug
   zou zijn geweest), en **de wereldpositie van "Floor (1)"** (nieuwste toevoeging, nog niet
   getest). **Volgende sessie: vraag de gebruiker om opnieuw te builden (OVR Build APK And
   Run), te spawnen, en de `[SpawnDebug]`-Logcat-output te verzamelen** — check specifiek de
   `Floor (1)`-wereld-Y-regel om TODO #5's laatste open hypothese af te ronden.
   Werkwijze die deze sessie goed werkte: `adb.exe logcat -d | grep -i SpawnDebug` via het pad
   in de "Build performance"-sectie hieronder (geen kabel-tethering aan een losse pc nodig,
   werkt direct via de al-USB-verbonden headset).
7. **Quest Link / MQDH-connectiviteit was deze sessie ook een obstakel.** In Meta Quest
   Developer Hub bleef de "Connect"-knop in het Link-paneel grijs/inactief (Status: Healthy,
   Device State: Not Connected, headset wél zichtbaar via ADB). Voorgestelde stappen (nog niet
   bevestigd of ze werkten): wifi/ethernet tijdelijk uitschakelen om de "OculusDash.exe
   probeert iets online te bereiken"-hypothese te testen (bekend, breed Link-probleem van
   januari 2026, toen opgelost via een Meta-update — kan terugkomen), Oculus VR Runtime
   Service herstarten, developer mode opnieuw bevestigen in de Meta Horizon mobiele app.
   **Nog te bevestigen bij de volgende sessie of Link inmiddels werkt**, en zo niet, of de
   gebruiker in plaats daarvan via een Android-build test.
   **Update (2026-09-19): op één Quest werkt Link inmiddels.** Nog niet bevestigd of dit ook
   geldt voor de andere headset(s)/machines — mogelijk dus nog steeds een obstakel op sommige
   Q's, nog te testen.
8. **Reminder: gebruiker wil later de "XR tools" in het Meta XR SDK-welkomstvenster bekijken**
   (Window → Meta XR → Welkomstscherm in Unity, of "Open SDK menu"). Tegels: AI Tools, Runtime
   optimizer, Immersive debugger, Meta XR Simulator, Meta Quest Developer Hub, Meta Quest Link,
   RenderDoc, Meta Haptics Studio. Nog niet besproken/geconfigureerd — puur een reminder,
   opgeslagen op 2026-09-18.
9. **"Missing Project ID"-dialoog bij klassiek builden naar Quest 3 (bevestigd 2026-09-18):**
   "Because you are not a member of this project this build will not access Unity services.
   Do you want to continue?" — **onschadelijk, gewoon op "Yes" klikken.** Komt door de
   `cloudProjectId`-koppeling (zie hierboven bij "Belangrijke technische feiten") gecombineerd
   met een editor-account dat geen lid is van dat Unity Cloud-project; heeft geen effect zolang
   `SessionType.LocalOnly` actief blijft.
10. **Compile-error opgelost (2026-09-18) — kan terugkomen na package-cache reset:** in
    `Library/PackageCache/com.meta.xr.sdk.core@.../Editor/BuildingBlocks/BlockData/
    MultiplayerBlocks/NGO/SceneListenerNGO.cs` gaf Meta's eigen SDK-code een `CS0619`-
    compile-error (`CreateGameObjectHierarchyEventArgs.instanceId` is obsolete sinds
    Unity 6000.3+, moet `.entityId` zijn). **Root cause: bug in Meta's SDK zelf**, niet in dit
    project — hun `#if UNITY_6000_3_OR_NEWER`-branch riep terecht de nieuwe
    `EditorUtility.EntityIdToObject(...)`-methode aan, maar las per ongeluk nog de oude
    `.instanceId`-property i.p.v. `.entityId` uit (onvolledige migratie). Dit blokkeerde
    **alle** builden (Unity build niet mogelijk bij eender welke compile-error, ongeacht
    menu/methode). **Fix toegepast:** `.instanceId` → `.entityId` op de twee betrokken regels
    in dat bestand (binnen de `UNITY_6000_3_OR_NEWER`-branches). Een vergelijkbaar bestand,
    `Editor/OVRTelemetry/OVRSceneChangeListener.cs`, heeft dezelfde legacy-regel maar wél al
    een correcte extra `#elif UNITY_6000_5_OR_NEWER`-branch ervóór (met `.entityId`) — die
    branch wordt bij dit project (Unity 6000.5.4f1) gebruikt, dus dat bestand faalde niet en
    is niet aangepast.
    ⚠️ **Deze patch zit in `Library/PackageCache` (Unity-beheerde cache) en is dus tijdelijk/
    lokaal** — bij "Reimport All", het verwijderen van de `Library`-map, of het updaten/
    opnieuw installeren van de Meta XR SDK-package (`com.meta.xr.sdk.all`) wordt dit bestand
    overschreven en moet de fix **opnieuw** toegepast worden (of controleren of Meta inmiddels
    een nieuwere SDK-versie met deze fix heeft uitgebracht). Nog niet gemeld bij Meta als bug.
    **Vervolg (2026-09-18):** na deze fix trad een nieuwe, latere build-fout op:
    `UnityLinker.exe` crashte met `System.AccessViolationException` in
    `Mono.Cecil.MetadataReader.ReadTypes()` tijdens de `ManagedStripped`-stap. Fix: `Library\Bee`
    verwijderd (veilig regenereerbare cache). **✅ Impliciet bevestigd opgelost:** na deze
    verwijdering zijn er verderop in de sessie meerdere succesvolle builds gedaan (OVR Build
    APK, herhaalde rebuilds voor de `[SpawnDebug]`-iteraties) zonder dat deze crash nog
    terugkwam.
11. ✅ **Afgehandeld (2026-09-18): Unity Cloud Services ontkoppeld.** Loste zowel de
    "Missing Project ID"-build-dialoog als de Vivox 403-startup-error op (zelfde grondoorzaak:
    `cloudProjectId` gekoppeld aan een account waar de Unity-login geen lid van was — bleek
    bovendien een verweesde koppeling naar een niet meer bestaand/toegankelijk cloud-project
    genaamd "VR_MP_01 2026-07-22_14-38-39", een leftover van vóór de hernoeming naar
    FreeRoamVR). Unity's eigen Services-paneel bood in deze kapotte staat geen werkende
    "Unlink"-knop (enkel "link naar een nieuw Project ID"), dus zijn `cloudProjectId`,
    `organizationId` en `projectName` rechtstreeks leeggemaakt in `ProjectSettings.asset`.
    Gebruiker bevestigde dat dit werkte. Geen effect op gameplay (`SessionType.LocalOnly`
    gebruikte UGS/Vivox toch al nooit).
12. ✅ **Afgehandeld (2026-09-19): `NullReferenceException` in `VoiceChatManager.Set3DAudio`
    opgelost.** Root cause bleek een mismatch tussen twee losse "sessietype"-bronnen:
    `XRINetworkGameManager.CurrentSessionType` staat hard op `SessionType.LocalOnly` (bewust,
    zie "LAN-specifieke aanpassingen"), maar `SessionManager.sessionType` is een **apart**,
    ouder serialized veld (`m_SessionType`) dat in `Assets/VRMPAssets/Prefabs/Managers/XRI
    Network Game Manager.prefab` nog op `0` (`DistributedAuthority`) stond. Daardoor riep
    `SessionManager.Start()` nooit `SetupLocalTransport()` aan — de enige plek die
    `positionalVoiceChat` op `false` zet — terwijl het prefab bovendien een verweesde override
    `positionalVoiceChat: 1` had (code-default is `false`). Gevolg: `XRINetworkPlayer.Update()`
    riep bij elke voldoende hoofdbeweging `Set3DAudio()` aan, terwijl `VoiceChatManager.Start()`
    zelf terecht de `LocalOnly`-guard volgt en Vivox nooit initialiseert — dus
    `VivoxService.Instance.IsLoggedIn` crashte.
    **Fix toegepast:** `positionalVoiceChat: 1` → `0` in dat prefab (terug naar code-default).
    Bewust **niet** `SessionManager.sessionType` zelf op `LocalOnly` gezet — dat zou ook
    `SetupLocalTransport()` triggeren (wijzigt `NetworkTransport`, destroyt de DA-transport),
    wat kan conflicteren met `AutoRoomManager`'s eigen werkende LAN-transportopzet. Geïsoleerde
    flag-fix was voldoende.
    **Getest via Quest Link (Play Mode), niet via build** — puur een prefab-databewerking,
    geen C#-wijziging, dus Play Mode leest de aangepaste asset direct; geen compile/IL2CPP-stap
    nodig om te verifiëren. **Bevestigd door gebruiker (2026-09-19): melding is weg.**
13. ✅ **Afgehandeld (2026-09-19): spawn/vloerhoogte-bug volledig opgelost**, zie TODO #5 voor
    de volledige analyse en fix ("Floor (1)" vervangen door een correct geplaatste Plane +
    Mesh Collider + Teleport Area). Bevestigd werkend door gebruiker op Q.
14. **Eerste keer Meta XR-tooling opzetten op Omen (2026-09-18):** tijdens het installeren van
    `com.meta.xr.sdk.all` bleef een achtergrondtaak "Meta XR Simulator Installer" hangen op 0%
    ("Not Responding" na ~13 min), met een tweede automatische herpoging die ook meteen leek te
    haperen. **Gebruiker heeft dit weggeklikt zonder dat de installatie voltooide** — nog niet
    dus zeker of de Simulator zelf goed is geïnstalleerd; hou hier rekening mee als er later
    Simulator-gerelateerde problemen optreden.
    Zoals verwacht (zie TODO #10) kwam **dezelfde `CS0619` `SceneListenerNGO.cs`-compile-error**
    ook op Omen terug (want de fix zit in `Library/PackageCache`, niet in git) — opnieuw
    toegepast (`.instanceId` → `.entityId` op de twee betrokken regels) en **bevestigd werkend**:
    compile-error weg, "OVR Build"-menu weer aanwezig. Bevestigt dat dit een betrouwbaar
    reproduceerbare per-machine-stap is, geen eenmalig toeval.
15. ✅ **Afgehandeld (2026-09-18): MQDH-update op Omen.** Bleef eerst hangen op "Install Now"
    (deed niets). **Fix: een herstart van Omen was voldoende** — update slaagde daarna gewoon.
    **Weerlegt de netwerk-/firewallhypothese** die hier eerder stond (mogelijk patroon met
    TODO #14's vastgelopen Simulator-download) — het lijkt eerder een vastgelopen lokaal
    achtergrondproces/lock te zijn geweest dan een blokkade richting Meta's servers. De
    vastgelopen Simulator-installer (TODO #14) blijft een apart, nog niet bevestigd punt: die
    is destijds weggeklikt zonder te voltooien, dus nog steeds de moeite waard om te checken
    (bv. via het Meta XR SDK-welkomstvenster, zie TODO #8) of de Simulator zelf wel goed
    geïnstalleerd is, al is een simpele herstart-als-eerste-troubleshoot-stap voortaan een
    goede eerste gok bij vastgelopen Meta-installers op Omen.
16. ✅ **Afgehandeld (2026-09-18/19): opnieuw een IL2CPP-crash op Omen, zelfde familie als
    TODO #10's `Library\Bee`-probleem, maar deze keer pas op de headset i.p.v. tijdens het
    builden.** Na de eerste succesvolle build op Omen crashte de app direct bij opstarten met
    `SIGABRT` in `il2cpp_init` (backtrace: `libunity.so InitializeIl2CppFromMain` →
    `libil2cpp.so` → `abort`) — dus vóórdat er ook maar één frame van de game draaide, wat
    bevestigde dat dit **niets met de Floor-fix te maken had**. Duidt op corrupte/inconsistente
    IL2CPP-metadata die toch in de APK terechtkwam. **Fix: `Library\Bee` opnieuw verwijderd**
    (Unity dicht, veilig regenereerbare cache) en volledig opnieuw gebouwd (~20 min, geen
    patch-build). **Bevestigd werkend (2026-09-19):** na de schone rebuild draait de app
    zonder crash. Les: dit `Library\Bee`-corruptiepatroon is dus niet eenmalig geweest bij de
    eerste keer opzetten op Omen — het kan blijkbaar opnieuw optreden na een volgende
    build-cyclus op dezelfde machine, dus bij een vergelijkbare crash (SIGABRT/il2cpp_init of
    UnityLinker-crash tijdens builden) is `Library\Bee` verwijderen + volledig herbouwen de
    eerste troubleshoot-stap, ongeacht welke machine.
    **Zijstap besproken:** Unity's **"Application Patching"** (patch-build i.p.v. volledige
    rebuild, via Build Settings → Android → Patch Package, of Meta's OVR Build met
    Gradle-cache) is een legitieme versnelling voor latere kleine iteraties, maar is bewust
    **niet** gebruikt bij het oplossen van déze crash — een patch-build zou de mogelijk
    corrupte intermediate build-artifacts hergebruiken i.p.v. echt schoon herbouwen.

## Build performance (Android/Quest builds duren ~20 min)

Onderzocht op 2026-09-16 n.a.v. trage builds. Kernpunten:

- **ADB-pad (handig voor troubleshooting, gevonden 2026-09-18):**
  `C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\
  platform-tools\adb.exe` — er staat geen losse `adb` op het systeem-PATH, gebruik dit volledige
  pad (bv. `& "<pad>" devices -l`) om te checken of de Quest via USB herkend wordt.

- **Mono ondersteunt geen ARM64 op Android** (bevestigd via Unity-forums/documentatie) — enkel
  IL2CPP kan ARM64 builden. Dit project staat op `AndroidTargetArchitectures: 2` (ARM64-only),
  dus Mono + ARM64 is technisch geen geldige combinatie in Unity.
- De **64-bit-eis van Meta is een App Lab/Store-submissiepolicy, geen OS-blokkade**: apps die
  vóór 19 dec 2019 als 32-bit werden ingediend mogen nog steeds als 32-bit updaten, dus de
  Quest-OS zelf draait nog altijd 32-bit (ARMv7) apps. Voor **lokaal sideloaden** (wat dit
  project doet — geen App Lab/Store) zou een Mono+ARMv7-build dus waarschijnlijk gewoon
  installeren en draaien op de headset.
- **Meta's eigen "Unity Iteration Speed Best Practices"-gids raadt Mono/ARMv7 echter nergens
  aan** als oplossing voor trage builds. Officieel aanbevolen aanpak (in volgorde van impact):
  1. **Meta Horizon Link of de Meta XR Simulator** gebruiken om zonder device-build te
     itereren (logica/positionering testen zonder elke keer te builden).
     ✅ **Uiteindelijk opgelost/verklaard (2026-09-18):** "OVR Build APK" bestaat wél degelijk
     — bevestigd via de daadwerkelijke package-broncode
     (`Library/PackageCache/com.meta.xr.sdk.core@.../Editor/OVRBuild.cs`), met menupad
     **Meta → Tools → OVR Build → OVR Build APK...** (Ctrl+Shift+K) / **OVR Build APK And
     Run** (Ctrl+K). Het item verscheen echter niet in het menu, ook niet na eerdere checks
     van het welkomstvenster en het "Tools"-submenu (die toen inderdaad geen "OVR Build"-item
     toonden). **Root cause: niet Addressables (die theorie was fout), maar de losstaande
     `CS0619`-compile-error** verderop in deze sectie (`SceneListenerNGO.cs`) — zolang die
     error ergens in het project bestond, registreerde Unity **domeinbreed geen enkel nieuw
     menu-item** correct (Unity blokkeert de volledige script-domain-reload bij eender welke
     compile-error, ongeacht in welke assembly). Na het patchen van die error (en het
     verwijderen van `Library\Bee`) verscheen "OVR Build" alsnog in het menu — bevestigd door
     de gebruiker. **Les voor volgende keer:** als een Meta XR SDK-menu-item onverklaarbaar
     ontbreekt, eerst checken op compile-errors in de Console (Window → General → Console)
     vóórdat je concludeert dat een pakket/tool niet bestaat of een andere dependency mist.
     ✅ **Bevestigd werkend (2026-09-18):** gebruiker heeft succesvol lokaal gebouwd met
     "OVR Build APK..." (Built APK Path ingevuld, "Install & Run on Device?" uitgevinkt/
     uitgegrijsd want geen ADB-device nodig voor een lokale build, "Development Build" aan
     aanbevolen i.v.m. de nog te verzamelen `[SpawnDebug]`-logs, zie TODO #6). De
     "No ADB devices connected"-Console-error bleek louter cosmetisch (telemetry-log in
     `OnEnable`/`CheckADBDevices`), geen blokkade — het bouwvenster en de Build-knop werken
     gewoon door zonder verbonden headset.
  2. Bij IL2CPP: **"Strip Engine Code" uit** (in dit project al `stripEngineCode: 0` — dus
     al correct ingesteld) + Unity's **script-only APK-patching** bij code-only wijzigingen
     (±45% sneller, vereist dat de eerste build al gedaan is).
  3. **Addressables** om assets los van code te builden (>50% sneller, tot ~75% bij enkel
     asset-wijzigingen).
  4. Nieuwere Unity LTS-versie (8-16% winst tussen major versions).
- **Aanbeveling:** scripting backend terugzetten naar IL2CPP (`Android: 1`) en inzetten op
  bovenstaande IL2CPP-vriendelijke technieken i.p.v. Mono, om het risico te vermijden dat een
  build niet (meer) correct installeert en om dicht bij een store-compatibele configuratie te
  blijven mocht dit project ooit toch naar App Lab moeten. **Nog niet uitgevoerd — actie nog
  te bevestigen met de gebruiker.**

## Opgeloste issues (afgevinkt)

- ✅ Y-positie van "FirstSpawnPoint" gecorrigeerd naar vloerhoogte (was `(0.54, 3.84, 3.19)`,
  nu `(-2, 0, 5)`). Geverifieerd op 2026-09-16.
- ✅ Player Settings bijgewerkt en opgeslagen: Company Name `SintLucas.IX`, Product Name
  `FreeRoamVR`, Version `0.0.1` (`ProjectSettings/ProjectSettings.asset`).
- ✅ Ongebruikte grote assets opgeruimd (2026-09-16, netto ~475,7 MB bespaard — zie
  correctie hieronder):
  - `Assets/Free 8 Spooky Tracks/` — 7 van de 8 tracks verwijderd (`Spooky 1,2,3,4,5,6,8.wav`,
    141,94 MB), `Spooky 7.wav` bleef staan (enige met een actieve referentie).
  - `Assets/ADG_Textures/walls_vol1/` (PBR-muurtexturenpakket) — 15 van de 18 wall-mappen
    verwijderd (~333,7 MB). **Correctie op de eerste analyse:** `wall01`, `wall02` en `wall11`
    zijn wél in gebruik in `SampleScene.unity` (materialen `wall01b.mat`, `wall02.mat`,
    `wall11b.mat`) en zijn na een "roze materialen"-melding teruggezet vanuit backup
    (36 MB, incl. hun textures). De eerste GUID-scan door een subagent had deze 3 over het
    hoofd gezien — **vertrouw bij toekomstige "ongebruikte assets"-analyses niet blind op
    een eerste scan; verifieer GUID-referenties in `.unity`-scenebestanden extra grondig
    voordat je iets verwijdert, zeker bij materialen die in meerdere varianten
    (bv. `wallXXb.mat`) per map voorkomen.**
  - Backups (buiten dit projectmap, voor het geval iets toch nodig blijkt) staan in
    `vakinhoud\ADG_Textures_backup_FreeRoamVR\` en `vakinhoud\Spooky_Tracks_backup_FreeRoamVR\`.
  - Na verwijderen/terugzetten van assets: Unity moet opnieuw geopend worden (of
    Assets → Reimport All) zodat de Library-cache de GUID's opnieuw oplijnt.
