
# Testscenarioer – CrisisSystem

## Om testingen

Formålet med testingen er å kontrollere at CrisisSystem fungerer som forventet, og at systemet håndterer både gyldige og ugyldige brukerhandlinger. Testscenarioene omfatter registrering og håndtering av ressurser og behov, kartfunksjonalitet, brukergrensesnitt, informasjonssider og autentisering.

Hvert testscenario dokumenteres med forutsetninger, teststeg, forventet resultat, faktisk resultat og teststatus. Eventuelle feil dokumenteres og testes på nytt etter utbedring.

**Teststatus:** Ikke testet · Bestått · Feilet · Verifisert OK

---

## Resource (Vetle)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| RES-01 | Registrere ny ressurs med gyldige data | Ingen innlogging implementert ennå, alle kan registrere | 1. Gå til Ressurser → Registrer ny<br>2. Fyll ut alle felt<br>3. Trykk "Registrer" | Ressursen lagres og vises på oversiktssiden | Lander på detaljside med informasjon om ressursen og mulighet til å slette. Ressursen vises også på oversiktssiden (/Resource). | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå, jf. gruppens plan. Retestes når innlogging er på plass. |
| RES-02 | Registrere ressurs uten påkrevd felt | Ingen innlogging implementert ennå, alle kan registrere | 1. La et påkrevd felt stå tomt<br>2. Trykk "Registrer" | Valideringsfeil vises, ressurs lagres ikke | Feilmelding vises ("Velg type ressurs" / "Velg sted"), skjemaet sendes ikke inn. Ressursen dukker ikke opp på oversiktssiden (/Resource) etterpå. | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå, jf. gruppens plan. Bør retestes når innlogging er på plass. |
| RES-03 | Slette egen ressurs | Ressurs er registrert. Ingen innlogging implementert ennå, så eierskap sjekkes ikke | 1. Gå til egen ressurs<br>2. Trykk "Slett" | Ressursen fjernes fra listen | Bekreftelsesdialog vises, ressursen fjernes fra oversiktssiden (/Resource) etter bekreftelse. | Bestått | Testet uten innlogging siden autentisering ikke er bygget ennå. Eierskapssjekk må legges inn i ResourceController.Delete når innlogging er på plass. |
| RES-04 | Kontrollere unik ressurs-ID etter sletting | Systemet tillater registrering og sletting av ressurser | 1. Registrer tre ressurser<br>2. Slett ressurs nr. 2<br>3. Registrer en ny ressurs<br>4. Åpne detaljvisningen for ressursene | Den nye ressursen får en unik ID. Detaljvisning og sletting påvirker riktig ressurs. | | Ikke testet | Læringsassistenten har påpekt at _resources.Count + 1 kan føre til dupliserte ID-er. |
| RES-05 | Registrere ressurs uten kartposisjon | Ressursregistreringen er tilgjengelig | 1. Åpne registreringsskjemaet for ressurser<br>2. Fyll ut øvrige påkrevde felt<br>3. La kartposisjon stå tom<br>4. Trykk "Registrer" | Hvis kartposisjon er påkrevd, vises en tydelig valideringsfeil og ressursen lagres ikke. Hvis kartposisjon er valgfri, informeres brukeren tydelig om dette. | | Ikke testet | Avklar med gruppen om kartposisjon skal være påkrevd eller valgfri. |
| RES-06 | Registrere ressurs med ugyldige koordinater | Koordinatfeltene kan valideres | 1. Åpne registreringsskjemaet<br>2. Oppgi en breddegrad utenfor -90 til 90 eller en lengdegrad utenfor -180 til 180<br>3. Fyll ut øvrige felt<br>4. Trykk "Registrer" | Ugyldige koordinater avvises med en tydelig feilmelding. | | Ikke testet | Kontroller at koordinatvalideringen er like tydelig som i behovsskjemaet. |
| RES-07 | Kontrollere lengdegrenser på tekstfelt | Ressursregistreringen er tilgjengelig | 1. Åpne registreringsskjemaet<br>2. Skriv inn tekst som overskrider den definerte lengdegrensen<br>3. Fyll ut øvrige felt<br>4. Trykk "Registrer" | Skjemaet avviser tekst som overskrider den definerte lengdegrensen og viser en tydelig feilmelding. | | Ikke testet | Lengdegrenser må defineres og implementeres før testen gjennomføres. |
| RES-08 | Kontrollere e-postvalidering | Ressursskjemaet har et e-postfelt | 1. Åpne registreringsskjemaet<br>2. Skriv inn en ugyldig e-postadresse<br>3. Fyll ut øvrige påkrevde felt<br>4. Trykk "Registrer" | Ugyldig e-postadresse avvises med en tydelig feilmelding. | | Ikke testet | Læringsassistenten anbefaler [EmailAddress] på e-postfeltet. |
| RES-09 | Kontrollere tilgjengelighetsperiode | Ressursskjemaet har feltene tilgjengelig-fra og tilgjengelig-til | 1. Åpne registreringsskjemaet<br>2. Velg en tilgjengelig-til-dato som er før tilgjengelig-fra<br>3. Fyll ut øvrige påkrevde felt<br>4. Trykk "Registrer" | Registreringen avvises, og brukeren får beskjed om at sluttdato ikke kan være før startdato. | | Ikke testet | Læringsassistenten har anbefalt kontroll av tilgjengelighetsperioden. |

## Need (Sindre)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| NEED-01 | Registrere nytt behov med prioritet | Appen kjører. Ingen innlogging er implementert | 1. Åpne Registrer behov<br>2. Fyll ut alle felt og velg Akutt<br>3. Velg kartpunkt og trykk Registrer<br>4. Åpne Kart | Behovet registreres og vises med rød markør på valgt sted i kart | Behovet ble registrert og vist med rød markør på riktig sted | Bestått |

## Kart (Marius)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| MAP-01 | Velge punkt i kart under registrering | Bruker er på registreringsskjema | 1. Klikk et punkt i kartet | Koordinater fylles automatisk inn i skjema | Koordinater fylles inn automatisk utifra hvor bruker trykker på kartet | Bestått | |
| MAP-02 | Se registrerte ressurser/behov på kart | Minst én ressurs og ett behov er registrert | 1. Gå til Kart-siden | Riktig fargekode vises per type (rød/gul/grønn/blå) | Riktig fargekode vises per type | Bestått | |

## Frontend / UI (Sarah og Emma)

| ID | Scenario | Forutsetninger                                                          | Steg | Forventet resultat | Faktisk resultat | Status      | Kommentar/Issue |
|---|---|-------------------------------------------------------------------------|---|--|---|-------------|---|
| UI-01 | Responsivitet på mobil (< 400px) | testes på mobil eller et minimert vindu på pc                           | 1. Åpne forsiden i mobilvisning i DevTools | rask respons tid og funker når trykket på | funket, men kartet blir et hakke treigere når zoomer inn og ut. | bestått     | |
| UI-02 | Responsivitet på nettbrett (~768px) | testes på pc i developertool i nettbrettmodus                           | 1. Åpne forsiden i nettbrettvisning | hastigheten er optimal, | hastighet funker som den skal | bestått     | |
| UI-03 | Tastaturnavigasjon | Ha en fungerende tastatur og skjerm                                     | 1. Naviger hele forsiden kun med Tab | vellykket opp og ned navigering | fikk til å navigere med tab | bestått     | |
| UI-04 | Utskrift av ressursliste | Ressurslisten er tilgjengelig og inneholder minst én registrert ressurs | 1. Åpne ressursliste<br>2. Ctrl+P / Skriv ut | essurslisten vises i utskriftsvennlig format uten at innhold blir kuttet bort. | | Ikke testet | |

## Informasjonssider (Emma)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| INFO-01 | Om-siden vises | Ingen | 1. Klikk "Om oss" i menyen | Om-siden vises med alle seksjoner | Om-siden vises med alle seksjoner. | Bestått | |
| INFO-02 | Hjelp-siden, spørsmål åpnes | Ingen | 1. Klikk "Hjelp" i menyen<br>2. Klikk på et spørsmål | Svaret åpnes, bare ett svar er åpent om gangen | Alle spørsmål er tydelige på skjermen. Når man trykker på et spørsmål, blir det mer tydelig enn resten. | Bestått | |
| INFO-03 | Kontaktskjema sendt tomt | Ingen | 1. Gå til Kontakt<br>2. Trykk "Send" uten å fylle ut | Rød feilmelding vises, brukeren blir på Kontakt-siden | Skjemaet blir ikke sendt, og man får opp en tydelig feilmelding. | Bestått | |
| INFO-04 | Kontaktskjema delvis utfylt | Ingen | 1. Fyll inn bare navn og e-post<br>2. Trykk "Send" | Feilmelding vises, navn og e-post står fortsatt i feltene | Navn og e-post står fortsatt i feltene og feilmelding vises. | Bestått | |
| INFO-05 | Kontaktskjema sendt riktig | Ingen | 1. Fyll ut alle feltene<br>2. Trykk "Send" | Bekreftelsessiden viser "Takk, [navn]!" og det som ble sendt | Bekreftelsesmeldingen kommer opp midt på skjermen. | Bestått | |
| INFO-06 | Informasjonssider på mobil | Ingen | 1. Åpne Om, Hjelp og Kontakt i mobilvisning i DevTools | Innhold legger seg under hverandre, knapper fyller bredden, ingen sideveis scroll | Tydelig og riktig formatering på mobil. | Bestått | |
| INFO-07 | Lenker mellom sidene | Ingen | 1. Klikk "Kontakt oss" på Om og Hjelp<br>2. Klikk "Tilbake til forsiden" på bekreftelsessiden | Hver knapp går til riktig side | Man kommer inn på riktig side. | Bestått | |

## Autentisering (når implementert)

| ID | Scenario | Forutsetninger | Steg | Forventet resultat | Faktisk resultat | Status | Kommentar/Issue |
|---|---|---|---|---|---|---|---|
| AUTH-01 | Innlogging med gyldige opplysninger | Bruker finnes i systemet | 1. Gå til Logg inn<br>2. Fyll inn gyldig brukernavn/passord | Bruker logges inn og sendes til forsiden | | Ikke testet | |
| AUTH-02 | Innlogging med ugyldig passord | Bruker finnes i systemet | 1. Fyll inn feil passord | Feilmelding vises, bruker logges ikke inn | | Ikke testet | |

---

## Oppsummering

| Kategori | Antall scenarioer | Bestått | Feilet | Verifisert OK |
|---|---|---|---|---|
| Resource | 9 | 3 | 0 | |
| Need | 1 | 1 | 0 | |
| Kart | 2 | 2 | 0 | |
| Frontend / UI | 4 | 3 | 0 | |
| Informasjonssider | 7 | 7 | 0 | |
| Autentisering | 2 | 0 | 0 | |
| **Totalt** | **25** | **16** | **0** | |

