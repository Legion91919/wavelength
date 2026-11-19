# Product brief

> **Showcase project.** Mzansi Indie Alliance (fictional), a collective of 300 independent South African labels is a fictional client and every name, user and number here is invented.
> Wavelength is an original music streaming service, built to show how the Baton model handles a consumer-scale product.

## Client
Mzansi Indie Alliance (fictional), a collective of 300 independent South African labels

## Problem
Independent labels earn little from global platforms and get no clear numbers on who listens, so they can't pay artists fairly or plan tours.

## Users
Listeners (daily, mobile, commuting); label managers (monthly royalty statements).

## Outcomes
Search results in under 300 ms; next track starts without a gap; every qualifying play counted once for royalties.

## Constraints
A play counts for royalties after 30 seconds; play events must be idempotent; statements reconcile to the cent.

## Out of scope (first release)
Anything not needed for the outcomes above in the first three slices.

## Traits (for the architecture advisor)
- MediaDelivery: "every play is an audio stream"
- ReadHeavy: "searching and listening far outweigh catalogue uploads"
- PersonalData: "listener accounts and listening history"
- MoneyMovement: "subscriptions and per-play royalty payouts to labels"
- SeveralBusinessAreas: "catalogue, playback, playlists, royalties"
- ExternalPartnerIntegrations: "label catalogue feeds and a payment provider"
- SmallTeam: "four engineers"

Global-scale profile used for the evolution plan: MediaDelivery, ReadHeavy, GlobalScale, PersonalData, MoneyMovement.
