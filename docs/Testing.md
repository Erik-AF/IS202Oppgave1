# Testing

## Omfang

Testene ligger i `CrisisSystem.Tests` og bruker xUnit 2.9.3. Moq brukes til å isolere controllere fra Identity og datalagring. Integrasjonstestene bruker ekte MariaDB med prosjektets migreringer. Playwright bruker Chromium til å teste hele tilbudsflyten i en egen app-prosess.

Utvidelsen på `test_expand-test-coverage` legger til **38 testmetoder med 52 testtilfeller**. En `[Theory]` kan gi flere testtilfeller. Det var 101 testtilfeller før utvidelsen, og det er nå 153.

| Nye tester | Metoder | Innhold |
|---|---:|---|
| AccountController | 12 | Admin og ukjente roller avvises, ugyldig registrering, begge tillatte roller, duplikat e-post, opprydding etter mislykket rolletildeling, feil passord, sperret konto, lokal returadresse, utlogging og passordtilbakestilling |
| Min side og profil | 5 | Egne oppføringer, fire ressurser på oversikten, admin sine egne behov, opptatt e-post, gyldige og ugyldige profilendringer for begge controllere |
| Standardroller og brukere | 2 | Manglende roller opprettes og eksisterende admin beholdes |
| Utviklings-e-post | 1 | Mottaker, emne og dekodet meldingsinnhold logges |
| Excel-eksport | 1 | Filtrering, XLSX-innhold, telefonnummer og formellignende tekst |
| HTTP-autorisasjon | 1 | Fem beskyttede endepunkter avviser anonym tilgang gjennom MVC-pipelinen |
| Allokering i MariaDB | 7 | Reservasjon, godkjenning, rollback, frigjøring, sletting av behov og samtidige tilbud |
| Varsler i MariaDB | 4 | Eierskap, admin, bare ventende tilbud, reduksjon og tom/ukjent bruker |
| Bilder i MariaDB | 2 | Lagring og erstatning av bytes, sletting av bilde og allokering ved ressurssletting |
| Repositoryer i MariaDB | 2 | Opprette, lese, oppdatere og slette på tvers av databasekontekster, relasjoner og ressurs-ID |
| Nettleser | 1 | Provider logger inn og tilbyr, eier godkjenner, varsel forsvinner og begge logger ut |

Fire eksisterende testmetoder er oppdatert. De to redigeringstestene kontrollerer nå at repositoryets lagringsmetode faktisk kalles. Kartbaserte tilbud testes med direkte POST til godkjenning. Den historiske migreringstesten har fått et mer presist navn.

De gamle testene av den statiske kartlisten var allerede fjernet i branchens utgangspunkt. De nye karttestene er beholdt. Ingen eksisterende tester er slettet i denne utvidelsen.

## Kjør uten Docker

Fra repoets rotmappe:

```powershell
dotnet test CrisisSystem.Tests/CrisisSystem.Tests.csproj --logger "trx;LogFileName=tests.trx" --results-directory TestResults
```

MariaDB- og nettlesertestene rapporteres som **Skipped** når deres miljøvariabler ikke er satt. Dette er ikke et bestått resultat for disse testene.

## Kjør med isolert MariaDB

Krever .NET 10, PowerShell 7 og Docker Desktop med Linux-containere. Docker må kjøre og være tilgjengelig for brukeren.

```powershell
pwsh ./scripts/Test-All.ps1
```

Skriptet starter en egen midlertidig MariaDB 11-container på en tilfeldig lokal port. Hver databasetest oppretter sin egen `crisis_test_<guid>`-database og sletter bare denne etterpå. Containeren fjernes ved avslutning. Appens eksisterende containere, volumer og databaser brukes ikke.

## Kjør alle tester, inkludert nettleseren

```powershell
pwsh ./scripts/Test-All.ps1 -Browser
```

Dette installerer Chromium hvis nødvendig. Nettlesertesten starter den faktiske webappen med egen testdatabase og egne testkontoer. Appen bygges først. Den bruker ikke en manuelt startet app. Dersom Windows blokkerer de bygde filene, må maskinens programkontroll avklares. Testoppsettet slår ikke av sikkerhetsinnstillinger.

For en allerede tilgjengelig, isolert testserver kan testene også kjøres med `TEST_MARIADB_CONNECTION` satt til en bruker som kan opprette og slette databaser. Nettleseren aktiveres med `RUN_BROWSER_TESTS=1`. Bruk aldri produksjonstilgang. Uten `--no-build` bygges testene på nytt før kjøring.

GitHub Actions er konfigurert til å starte MariaDB, installere Chromium og kjøre alle testene. TRX-resultater og eventuelle nettleserbilder lastes opp som `test-results`. Denne CI-konfigurasjonen er ikke kjørt lokalt eller publisert i denne arbeidsøkten.

## Resultater fra 26.11.2026

| Kjøring | Bestått | Feilet | Hoppet over | Totalt |
|---|---:|---:|---:|---:|
| Før utvidelsen | 101 | 0 | 0 | 101 |
| Etter utvidelsen | 137 | 0 | 16 | 153 |

De 16 som ikke er kjørt er 15 MariaDB-tester og én nettlesertest. Docker API var ikke tilgjengelig fra arbeidsmiljøet. De er implementert og kompilert, men database- og nettleseroppførselen må fortsatt verifiseres med kommandoen over eller i GitHub Actions.

Lokale resultatfiler fra denne økten: `TestResults/baseline/baseline.trx` og `TestResults/current/final.trx`. Resultatfiler er ignorert av Git. Kopier dem til innleveringen eller hent CI-artifakten når den finnes. Ikke før opp en test som bestått uten faktisk testkjøring.

Byggingen varslet om en eksisterende MySqlConnector-avhengighet med NU1902. Avhengigheten i applikasjonen er ikke endret som del av testarbeidet.

## Begrensninger

- Enhetstester av Identity bruker mocks. De kontrollerer controllerens valg og kall, ikke selve passordalgoritmen eller ekte tokenutløp.
- HTTP-testen bruker ekte MVC-filtre og cookie-autentisering i en minimal testvert. Den starter ikke hele `Program.cs`. Nettlesertesten starter hele appen.
- En grønn test av et mock-kall beviser ikke at databaseendringen skjer. MariaDB-testene er nødvendig supplement.
- Den samtidige testen gjelder ressurstilbud via `OfferAsync`, ikke alle mulige konkurrerende operasjoner.
- Dette er ikke en full tilgjengelighets-, sikkerhets- eller ytelsesrevisjon. De praktiske testene nedenfor må også gjennomføres.

## Praktiske testscenarioer

Disse er en plan. Ingen av dem er gjennomført som del av denne endringen. Registrer maskin, operativsystem, nettleser, dato, tester, forventet og faktisk resultat, skjermbilder og eventuell retest.

| ID | Forutsetning og steg | Forventet resultat | Faktisk resultat | Status |
|---|---|---|---|---|
| MAN-01 Ny maskin og sensorguide | En person uten prosjektkunnskap følger README fra kloning til oppstart med Docker/Aspire. Prøv Windows med Smart App Control aktivert og noter installerte versjoner. | Guiden gir fungerende oppstart uten udokumenterte steg. Eventuell programkontrollblokkering dokumenteres som feil eller begrensning. | Ikke gjennomført | Ikke testet |
| MAN-02 Tom database | Bruk separat testmiljø uten gamle volumer. Start den komplette løsningen, registrer bruker og logg inn. | Migreringer, kategorier og roller opprettes. Brukeren kan registrere behov eller ressurs med sin rolle. | Ikke gjennomført | Ikke testet |
| MAN-03 Omstart | Registrer behov, ressurs og bilde i testmiljøet. Stopp og start app og database uten å slette volumene. | Alle oppføringer og bildet finnes fortsatt og kan åpnes. | Ikke gjennomført | Ikke testet |
| MAN-04 Brukere og roller | Følg dokumentasjonen for provider, offentlig aktør og admin. Prøv riktige og feil handlinger, også ved å åpne adresser direkte. | Rett bruker får rett tilgang. Dokumenterte kontoer eller registreringssteg fungerer. Uvedkommende kan ikke endre andres data. | Ikke gjennomført | Ikke testet |
| MAN-05 Brukervennlighet | La minst tre nye brukere registrere behov, finne en ressurs og behandle et tilbud uten hjelp. Mål tid, feilklikk og behov for veiledning. Bruk et rolig, simulert krisescenario. | Brukerne finner hovedhandlingene og forstår status og bekreftelser. Sett et konkret tidsmål før testen og dokumenter hver deltakers resultat. | Ikke gjennomført | Ikke testet |
| MAN-06 Tilgjengelighet | Test tastatur, synlig fokus, zoom 200 %, smal mobilbredde og kontrast. Prøv menyer, skjema, kartvalg og varsler. | Viktige handlinger kan nås uten mus, innhold overlapper ikke og status forklares også med tekst. Noter målte kontrastverdier og faktiske skjermstørrelser. | Ikke gjennomført | Ikke testet |

Historiske resultater i `Testscenarioer.md` beholdes som historikk. Beskrivelser der autentisering ikke var implementert, er ikke dokumentasjon på dagens tilgangskontroll. Gjennomfør nye tester før innleveringen.
