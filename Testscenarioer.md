# Testscenarioer – CrisisSystem

Eier: Emma. Alle gruppemedlemmer fyller ut egne rader når de tester sin funksjonalitet; Emma samler, strukturerer og fører formell test i tillegg.

## Slik brukes malen

1. Kopier én rad fra tabellen under per scenario du tester.
2. Fyll ut alle felt FØR du tester (Forutsetninger, Steg, Forventet resultat).
3. Test, og fyll inn Faktisk resultat og Status etterpå.
4. Hvis Status er "Feilet": opprett et GitHub Issue og lim inn lenken i "Kommentar/Issue"-feltet.
5. Emma verifiserer rettede feil og oppdaterer Status til "Verifisert OK".

**Statusverdier:** Ikke testet · Bestått · Feilet · Verifisert OK

---

## Resource (Vetle)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| RES-01 | Registrere ny ressurs med gyldige data | Ingen innlogging implementert ennå, alle kan registrere | 1. Gå til Ressurser → Registrer ny<br>2. Fyll ut alle felt<br>3. Trykk "Registrer" | Ressursen lagres og vises på oversiktssiden | Lander på detaljside med informasjon om ressursen og mulighet til å slette. Ressursen vises også på oversiktssiden (/Resource). | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå, jf. gruppens plan. Retestes når innlogging er på plass. |
| RES-02 | Registrere ressurs uten påkrevd felt | Ingen innlogging implementert ennå, alle kan registrere | 1. La et påkrevd felt stå tomt<br>2. Trykk "Registrer" | Valideringsfeil vises, ressurs lagres ikke | Feilmelding vises ("Velg type ressurs" / "Velg sted"), skjemaet sendes ikke inn. Ressursen dukker ikke opp på oversiktssiden (/Resource) etterpå. | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå, jf. gruppens plan. Bør retestes når innlogging er på plass. |
| RES-03 | Slette egen ressurs | Ressurs er registrert. Ingen innlogging implementert ennå, så eierskap sjekkes ikke | 1. Gå til egen ressurs<br>2. Trykk "Slett" | Ressursen fjernes fra listen | Bekreftelsesdialog vises, ressursen fjernes fra oversiktssiden (/Resource) etter bekreftelse. | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå. Eierskapssjekk må legges inn i ResourceController.Delete når innlogging er på plass. |

## Need (Sindre)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| NEED-01 | Registrere nytt behov med prioritet | Bruker er innlogget som offentlig aktør | 1. Gå til Behov → Registrer nytt<br>2. Sett prioritet = Akutt<br>3. Fyll ut resten og trykk "Registrer" | Behovet lagres og vises med rød markør i kartet | | Ikke testet | |
| NEED-02 | Endre status på behov | Et behov er registrert | 1. Åpne behovet<br>2. Endre status fra "Ny" til "Vurderes"<br>3. Lagre | Ny status vises på behovet | | Ikke testet | |

## Kart (Marius)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| MAP-01 | Velge punkt i kart under registrering | Bruker er på registreringsskjema | 1. Klikk et punkt i kartet | Koordinater fylles automatisk inn i skjema | | Ikke testet | |
| MAP-02 | Se registrerte ressurser/behov på kart | Minst én ressurs og ett behov er registrert | 1. Gå til Kart-siden | Riktig fargekode vises per type (rød/gul/grønn/blå) | | Ikke testet | |

## Frontend / UI (Sarah og Emma)

| ID | Scenario | Forutsetninger                                | Steg | Forventet resultat                        | Faktisk resultat | Status      | Kommentar/Issue |
|---|---|-----------------------------------------------|---|-------------------------------------------|---|-------------|---|
| UI-01 | Responsivitet på mobil (< 400px) | testes på mobil eller et minimert vindu på pc | 1. Åpne forsiden i mobilvisning i DevTools | rask respons tid og funker når trykket på |funket, men kartet blir et hakke treigere når zoomer inn og ut. | bestått     | |
| UI-02 | Responsivitet på nettbrett (~768px) | testes på pc i developertool i nettbrettmodus | 1. Åpne forsiden i nettbrettvisning | hastigheten er optimal,                   |hastighet funker som den skal | bestått     | |
| UI-03 | Tastaturnavigasjon | Ha en fungerende tastatur og skjerm           | 1. Naviger hele forsiden kun med Tab | vellykket opp og ned navigering           |fikk til å navigere med tab | bestått     | |
| UI-04 | Utskrift av ressursliste | ikke testet                                   | 1. Åpne ressursliste<br>2. Ctrl+P / Skriv ut | ikke testet                               | | Ikke testet | |

## Informasjonssider (Emma)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status  | Kommentar/Issue |
|---|---|---|---|---|---|---------|---|
| INFO-01 | Om-siden vises | Ingen | 1. Klikk "Om oss" i menyen | Om-siden vises med alle seksjoner | | Bestått | |
| INFO-02 | Hjelp-siden, spørsmål åpnes | Ingen | 1. Klikk "Hjelp" i menyen<br>2. Klikk på et spørsmål | Svaret åpnes, bare ett svar er åpent om gangen | | Bestått | |
| INFO-03 | Kontaktskjema sendt tomt | Ingen | 1. Gå til Kontakt<br>2. Trykk "Send" uten å fylle ut | Rød feilmelding vises, brukeren blir på Kontakt-siden | | Bestått | |
| INFO-04 | Kontaktskjema delvis utfylt | Ingen | 1. Fyll inn bare navn og e-post<br>2. Trykk "Send" | Feilmelding vises, navn og e-post står fortsatt i feltene | | Bestått | |
| INFO-05 | Kontaktskjema sendt riktig | Ingen | 1. Fyll ut alle feltene<br>2. Trykk "Send" | Bekreftelsessiden viser "Takk, [navn]!" og det som ble sendt | | Bestått | |
| INFO-06 | Informasjonssider på mobil | Ingen | 1. Åpne Om, Hjelp og Kontakt i mobilvisning i DevTools | Innhold legger seg under hverandre, knapper fyller bredden, ingen sideveis scroll | | Bestått | |
| INFO-07 | Lenker mellom sidene | Ingen | 1. Klikk "Kontakt oss" på Om og Hjelp<br>2. Klikk "Tilbake til forsiden" på bekreftelsessiden | Hver knapp går til riktig side | | Bestått | |

## Autentisering (når implementert)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| AUTH-01 | Innlogging med gyldige opplysninger | Bruker finnes i systemet | 1. Gå til Logg inn<br>2. Fyll inn gyldig brukernavn/passord | Bruker logges inn og sendes til forsiden | | Ikke testet | |
| AUTH-02 | Innlogging med ugyldig passord | Bruker finnes i systemet | 1. Fyll inn feil passord | Feilmelding vises, bruker logges ikke inn | | Ikke testet | |

---

## Oppsummering (fylles ut av Emma før innlevering)

| Kategori | Antall scenarioer | Bestått | Feilet | Verifisert OK |
|---|---|---------|--------|---|
| Resource | 3 |         |        | |
| Need | 2 |         |        | |
| Kart | 2 |         |        | |
| Frontend / UI | 4 |         |        | |
| Informasjonssider | 7 | 7       | 0      | |
| Autentisering | 2 |         |        | |
| **Totalt** | **20** |         |        | |
