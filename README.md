# FreeRoamVR — voor ontwerpers

Welkom! Dit is de VR social/mini-games-app waar we samen aan bouwen. Jouw taak als ontwerper:
de scene en de beleving vormgeven — 3D-modellen plaatsen, textures toepassen, audio toevoegen,
kortom: er een plek van maken die leuk is om in rond te lopen.

## Aan de slag

1. Zorg dat je Unity **6000.5.4f1** hebt (via Unity Hub).
2. Clone deze repo:
   ```
   git clone https://github.com/Luc-Peersman/FreeRoamVR-Main.git
   ```
3. Maak je eigen branch, met je naam erin:
   ```
   git checkout -b ontwerper/<jouw-naam>
   ```
4. Open het project in Unity en open `Assets/Scenes/SampleScene.unity`.

## Het voorbeeldhuisje

In de scene staat al een klein huisje: **`ImmersiveRoom_SLIX`** (te vinden in de Hierarchy).
Start Play Mode (of bouw naar je headset) en loop er doorheen — dit laat meteen zien wat
FreeRoamVR is: een virtuele ruimte waar je fysiek doorheen kan lopen.

**Dit huisje is jouw startpunt, geen eindresultaat.** Je mag:
- het aanpassen (andere meubels, andere kleuren, andere indeling), of
- het volledig verwijderen en vervangen door iets compleet eigens.

Beide is prima — zolang je maar **binnen die ene GameObject in de Hierarchy blijft werken**.
Hernoem `ImmersiveRoom_SLIX` gerust naar iets van jezelf (bv. `Content_<jouw-naam>`), maar houd
al je content onder dat ene root-object. Zo kan je werk later makkelijk uit je project gehaald
en in het hoofdproject geplaatst worden, zonder dat het door elkaar loopt met wat andere
ontwerpers gemaakt hebben.

Wat je **niet** hoeft aan te passen (en beter met rust laat): scripts, de XR-rig/spawnpunt,
`ProjectSettings`, of andere objecten in de scene buiten je eigen root-object.

## Je werk inleveren

1. Commit regelmatig, met korte, duidelijke boodschappen:
   ```
   git add .
   git commit -m "Meubels toegevoegd aan de woonkamer"
   ```
2. Push je branch:
   ```
   git push -u origin ontwerper/<jouw-naam>
   ```
3. Maak op GitHub een Pull Request aan tegen `main` — dat is het seintje dat je werk klaar is
   om opgehaald te worden.

Je hoeft zelf niets te mergen; dat gebeurt centraal.

### Extra: exporteer ook een .unitypackage

Naast pushen naar git, graag ook je werk exporteren als los `.unitypackage`-bestand — dat maakt
het voor de technicus die alles samenvoegt een stuk betrouwbaarder dan zelf door je project
zoeken:

1. Selecteer je root-GameObject in de Hierarchy (`ImmersiveRoom_SLIX` of hoe je 'm hernoemd hebt).
2. Rechtermuisknop → **Export Package...**
3. Laat **Include Dependencies** aangevinkt — zo komen je modellen, textures en audio automatisch
   mee, je hoeft niks apart te verzamelen.
4. Sla op als `Export/Content_<jouw-naam>.unitypackage` (maak de map `Export/` aan als die er nog
   niet is) en commit dat bestand mee in je branch.
