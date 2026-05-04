# TODO: Fix Lot Edit Forms Pre-fill

## Steps:
- [x] 1. Update `src/app/features/lots/lot-form/lot-form.component.ts`: Add edit mode detection (`route.params['numero']`), `isEditMode`, `loadLotData()` with `LotService.getLotByNumero()` and `patchValue`, update `onSubmit()`, add helpers (`isInvalid`, `formatDate?`).

- [x] 2. Update `src/app/features/lots/lot-form/lot-form.component.html`: Dynamic titles/buttons based on `isEditMode`, `@if` syntax, readonly numero in edit, validation styling.

**Status:** Frontend updates complete. Ready for testing.
- [ ] 2. Update `src/app/features/lots/lot-form/lot-form.component.html`: Dynamic titles/buttons based on `isEditMode`, `@if` syntax, readonly numero in edit, validation styling.
- [ ] 3. Test: `cd Cynapharm && ng serve`, navigate to `/lots/{existing_numero}/edit`, verify form pre-filled, submit updates data.
- [ ] 4. Clean up TODO.md

**Status:** Starting implementation...

