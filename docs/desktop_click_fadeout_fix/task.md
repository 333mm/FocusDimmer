# Task List: Fix Desktop Click Fadeout

- [x] Investigation & Diagnosis <!-- id: 0 -->
  - [x] Analyze `MainWindow.xaml.cs` for desktop click handling.
  - [x] Analyze `DimmerOverlay.cs` for animation logic.
  - [x] Identify potential cause: Intermediate states or missing animation trigger on window change.
- [ ] Implementation Planning <!-- id: 1 -->
  - [ ] Create `implementation_plan.md`.
- [ ] Execution <!-- id: 2 -->
  - [ ] Modify `FocusDimmer2/Components/DimmerOverlay.cs` to ensure `FadeToTransparent` is called when `windowChanged` is true, even if `shouldDim` is already false.
- [ ] Verification <!-- id: 3 -->
  - [ ] Create `walkthrough.md` explaining the fix and verification steps.
