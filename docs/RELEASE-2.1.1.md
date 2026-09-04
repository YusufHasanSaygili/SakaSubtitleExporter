# Saka 2.1.1

The episode queue now scrolls in pixels instead of jumping by whole episodes. Mouse-wheel input eases over 170 milliseconds; dragging the scrollbar and keyboard navigation remain immediate. Reversing the wheel changes direction without waiting for the previous movement. Windows wheel-line and animation preferences are respected, and list virtualization stays enabled.

The native scrollbar has been replaced with a slim blue thumb that follows the selected theme. The MKV drop area has a cleaner file card, clearer instructions and a highlighted state when valid files are dragged over the window.

Tests cover intermediate animation frames, the live animation timer, wheel accumulation and reversal, thumb dragging, scroll boundaries, reduced motion, drop feedback, both themes and the existing extraction workflow.
