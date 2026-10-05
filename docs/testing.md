# Matrimony Hub - Testing Strategy & Verification Report

## 1. Overview
The test suite in `tests/MatrimonyHub.Tests/` provides automated verification of core business invariants, security constraints, and integration pathways.

## 2. Test Execution
To execute the test suite:
```bash
dotnet test
```

## 3. Verified Security & Business Invariants

| Test Name | Rule Verified | Result |
| :--- | :--- | :--- |
| `UserCannotAccessContactInfoWithoutSuccessfulPayment` | Unauthenticated / unpaid users receive NULL phone/email | **PASSED** |
| `UserCanAccessContactInfoAfterSuccessfulPayment` | Legitimate payer receives unlocked phone and email | **PASSED** |
| `AdminCannotApproveOwnNidVerification` | Admin is blocked from approving their own NID verification | **PASSED** |
| `PrivateNidInformationIsNotExposedInPublicProfileDto` | DTOs do not contain NID numbers or scan documents | **PASSED** |
| `ToggleFavorite_PreventsSelfFavoriting` | Users cannot add their own profile to favorites | **PASSED** |
| `MatchScoring_AccuratelyWeightsMatchingAttributes` | Algorithm scores high for compatibility, low for mismatch | **PASSED** |
| `HomePage_ReturnsSuccessAndCorrectContentType` | Web application serves valid HTML landing page | **PASSED** |
| `FindMatchesPage_ReturnsSuccess` | Match search page loads with functional filter parameters | **PASSED** |
| `SuccessStoriesPage_ReturnsSuccess` | Public success stories page renders approved stories | **PASSED** |
| `AdminPortal_RequiresAuthentication_RedirectsUnauthenticatedUser` | Unauthenticated visits to `/Admin` redirect to `/Auth/Login` | **PASSED** |
