# FreeRoamVR — voor ontwerpers

Welkom! Dit is de VR-app waar we samen aan bouwen. Jouw taak als ontwerper: de scene en de
beleving vormgeven — 3D-modellen plaatsen, textures toepassen, audio toevoegen, kortom: er een
plek van maken die leuk is om in rond te lopen.

Je gebruikt hiervoor twee programma's:
- **Unity** — om de 3D-scene te bouwen.
- **Visual Studio Code (VSCode)** — om je werk op te slaan en naar GitHub te sturen, zodat wij
  het van je kunnen ophalen. Al het "GitHub-werk" in dit document doe je in VSCode, met knopjes
  en menu's — je hoeft geen commando's te typen.

Nooit met Git/GitHub gewerkt? Geen probleem, dit document legt letterlijk elke klik uit.

## 0. Eenmalig installeren (voor je begint)

1. **Git** — download en installeer van [git-scm.com](https://git-scm.com/downloads). Bij de
   installatie steeds gewoon op "Next"/"Install" klikken, de standaardopties zijn prima.
2. **Visual Studio Code** — download en installeer van [code.visualstudio.com](https://code.visualstudio.com/).
3. **Een GitHub-account** — maak er gratis een aan op [github.com](https://github.com) als je er
   nog geen hebt.
4. **Unity Hub + Unity 6000.5.4f1** — via Unity Hub de juiste versie installeren.

Controleren of Git goed geïnstalleerd is: open VSCode, ga naar het menu **Terminal → New
Terminal** (bovenin het venster), typ `git --version` en druk op Enter. Zie je een versienummer
verschijnen? Dan is het goed gegaan.

## 1. Het project ophalen ("clonen")

1. Open VSCode.
2. Druk op **Ctrl+Shift+P** — dit opent een zoekbalk bovenin (de "Command Palette").
3. Typ: `Git: Clone` en klik op de optie die verschijnt.
4. Kies **"Clone from GitHub"**.
   - Eerste keer? Dan vraagt VSCode om in te loggen. Klik **"Allow"**, er opent een
     browservenster — log in op GitHub en klik op **"Authorize Visual Studio Code"**. Ga daarna
     terug naar VSCode.
5. Typ in het zoekveld: `Luc-Peersman/FreeRoamVR-Main` en klik de repo aan in de lijst die
   verschijnt.
6. Kies een map op je computer waar het project moet komen (bv. je Documenten-map) en klik
   **"Select as Repository Destination"**.
7. Zodra het klaar is, verschijnt rechtsonder een pop-up met de vraag of je de map wil openen.
   Klik **"Open"**.

Je hebt nu een eigen kopie van het project op je computer staan.

## 2. Je eigen branch aanmaken

Een "branch" is jouw eigen, aparte werkkopie — zo kunnen alle ontwerpers tegelijk werken zonder
elkaar in de weg te zitten.

1. Kijk helemaal linksonder in het VSCode-venster: daar staat een vertakkingsicoon met de tekst
   **main**.
2. Klik daarop.
3. Kies bovenaan de lijst **"Create new branch..."**.
4. Typ als naam: `ontwerper/<jouw-naam>` — dus bijvoorbeeld `ontwerper/lisa` — en druk op Enter.

Linksonder staat nu jouw eigen branchnaam in plaats van "main". Vanaf nu werk je altijd hierin.

## 3. Het project openen in Unity

1. Open **Unity Hub**.
2. Klik **"Add"** → **"Add project from disk"**.
3. Blader naar de map waar je in stap 1 het project hebt neergezet en selecteer die.
4. Open het project (Unity-versie **6000.5.4f1**).
5. Open in Unity, via het **Project**-paneel onderin, de scene:
   `Assets/Scenes/SampleScene.unity` (dubbelklikken).

## 4. Het voorbeeldhuisje

In de scene staat al een klein huisje: **`ImmersiveRoom_SLIX`** (te vinden in het
**Hierarchy**-paneel, meestal links in het Unity-venster).

Start Play Mode (het driehoekje ▶ bovenaan) en loop er doorheen — dit laat meteen zien wat
FreeRoamVR is: een virtuele ruimte waar je fysiek doorheen kan lopen.

**Dit huisje is jouw startpunt, geen eindresultaat.** Je mag:
- het aanpassen (andere meubels, andere kleuren, andere indeling), of
- het volledig verwijderen en vervangen door iets compleet eigens.

Beide is prima — zolang je maar **binnen die ene GameObject in de Hierarchy blijft werken**.
Hernoem `ImmersiveRoom_SLIX` gerust naar iets van jezelf (bv. `Content_lisa`), maar houd al je
content onder dat ene root-object (dus als kind-object erin, of er direct op). Zo kan je werk
straks makkelijk uit je project gehaald worden zonder dat het door elkaar loopt met wat andere
ontwerpers gemaakt hebben.

Wat je **niet** hoeft aan te passen (en beter met rust laat): scripts, de XR-rig/spawnpunt,
`ProjectSettings`, of andere objecten in de scene buiten je eigen root-object.

## 5. Je werk tussentijds opslaan (committen)

Doe dit regelmatig, niet pas aan het einde — zo verlies je nooit veel werk.

1. Ga terug naar VSCode.
2. Klik links in de zijbalk op het **Source Control**-icoon (de derde of vierde knop van boven,
   ziet eruit als een vertakking), of druk op **Ctrl+Shift+G**.
3. Je ziet een lijst met alle bestanden die je hebt aangepast sinds de vorige keer.
4. Typ bovenaan in het tekstvak ("Message") een korte omschrijving van wat je gedaan hebt, bv.:
   `Meubels toegevoegd aan de woonkamer`.
5. Klik op het **vinkje (✓)** bovenaan het Source Control-paneel. Dit heet "Commit" en slaat je
   wijzigingen lokaal op (nog niet naar GitHub — dat is de volgende stap).

## 6. Je werk versturen naar GitHub (pushen)

1. Nog steeds in het Source Control-paneel: klik op de knop **"Publish Branch"** (de eerste keer)
   of **"Sync Changes"** (daarna).
2. Wacht tot het balkje/animatie klaar is — je werk staat nu op GitHub.

Herhaal stap 5 en 6 zo vaak als je wil terwijl je verder bouwt.

## 7. Je werk inleveren (Pull Request)

Als je klaar bent (of een tussenversie wil inleveren):

1. Ga naar [github.com](https://github.com) in je browser en log in.
2. Ga naar `github.com/Luc-Peersman/FreeRoamVR-Main`.
3. Je ziet bovenaan een gele balk met jouw branchnaam en de knop **"Compare & pull request"** —
   klik daarop.
4. Klik op de groene knop **"Create pull request"**.

Dat is het seintje voor de technicus dat jouw werk klaar is om opgehaald te worden. Je hoeft zelf
niets te mergen.

## 8. Extra: exporteer ook een .unitypackage

Dit maakt het voor de technicus een stuk betrouwbaarder dan zelf door jouw project zoeken:

> **Let op:** de exportoptie (in oudere Unity-tutorials vaak "Export Package..." genoemd, heet
> in deze Unity-versie **"Export Assets..."**) staat alleen in het rechtermuisknop-menu van het
> **Project**-paneel (op een bestand), niet in het **Hierarchy**-paneel (op een object in de
> scene). Je root-object is nog geen eigen bestand, dus je maakt er eerst een prefab van
> (stap 1-2 hieronder) voor je kan exporteren.

1. Als je root-GameObject (`ImmersiveRoom_SLIX` of hoe je 'm hernoemd hebt) in de Hierarchy
   **blauw** wordt weergegeven en binnen een ander prefab genest zit (bv. onder "Level"), moet
   je dat eerst loskoppelen: rechtermuisknop op de **root van die prefab-instance** (bv.
   "Level", niet je eigen object) → **Prefab → Unpack**. Zonder dit geeft Unity de foutmelding
   "Cannot restructure Prefab instance" bij de volgende stap.
2. Sleep je root-GameObject vanuit het **Hierarchy**-paneel naar een map in het
   **Project**-paneel, bv. `Assets/StudentContent/` (maak die map eerst aan als die er nog niet
   is). Hiermee maak je er een `.prefab`-bestand van; het object in je scene wordt nu blauw
   (= gekoppeld aan die prefab) en je kan er gewoon in blijven werken.
3. Klik in het **Project**-paneel (niet de Hierarchy!) op dat nieuwe prefab-bestand.
4. Rechtermuisknop erop → **Export Assets...**
5. Laat het vinkje bij **"Include Dependencies"** aan staan — zo komen je modellen, textures en
   audio automatisch mee.
6. Klik **Export...**, en sla het bestand op als `Export/Content_<jouw-naam>.unitypackage`
   (maak in het bestandsvenster eerst de map `Export` aan als die er nog niet is, via
   "Nieuwe map").
7. Ga terug naar VSCode, herhaal stap 5 en 6 uit hoofdstuk 5/6 hierboven (committen + pushen)
   zodat ook dit bestand meegaat.
