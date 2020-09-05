#include <vector>
#include "mediapipe_api/framework/formats/rect.h"

void MpRectDestroy(MpRect* rect) {
  delete rect;
}

void MpRectVectorDestroy(MpRectVector* rect_vector) {
  delete rect_vector;
}

MpRect* MpRectVectorRects(MpRectVector* rect_vector) {
  return rect_vector->rects;
}

int MpRectVectorSize(MpRectVector* rect_vector) {
  return rect_vector->size;
}

void MpNormalizedRectDestroy(MpNormalizedRect* rect) {
  delete rect;
}

void MpNormalizedRectVectorDestroy(MpNormalizedRectVector* rect_vec) {
  delete rect_vec;
}

MpNormalizedRect* MpNormalizedRectVectorRects(MpNormalizedRectVector* rect_vec) {
  return rect_vec->rects;
}

int MpNormalizedRectVectorSize(MpNormalizedRectVector* rect_vec) {
  return rect_vec->size;
}

template <typename T, class Rect>
T* MpPacketGetRectImpl(MpPacket* packet) {
  auto& rect = packet->impl->Get<Rect>();

  return new T {
    rect.x_center(),
    rect.y_center(),
    rect.height(),
    rect.width(),
    rect.rotation(),
    rect.rect_id()
  };
}

MpRect* MpPacketGetRect(MpPacket* packet) {
  return MpPacketGetRectImpl<MpRect, mediapipe::Rect>(packet);
}

MpNormalizedRect* MpPacketGetNormalizedRect(MpPacket* packet) {
  return MpPacketGetRectImpl<MpNormalizedRect, mediapipe::NormalizedRect>(packet);
}

template <typename Vec, typename Elem, class Rect>
Vec* MpPacketGetRectVectorImpl(MpPacket* packet) {
  auto& rect_vec = packet->impl->Get<std::vector<Rect>>();
  int size = rect_vec.size();

  Elem* rects = new Elem[size];
  int i = 0;

  for (auto& rect : rect_vec) {
    rects[i++] = Elem {
      rect.x_center(),
      rect.y_center(),
      rect.height(),
      rect.width(),
      rect.rotation(),
      rect.rect_id()
    };
  }

  return new Vec { rects, size };
}

inline MpRectVector* MpPacketGetRectVector(MpPacket* packet) {
  return MpPacketGetRectVectorImpl<MpRectVector, MpRect, mediapipe::Rect>(packet);
}

inline MpNormalizedRectVector* MpPacketGetNormalizedRectVector(MpPacket* packet) {
  return MpPacketGetRectVectorImpl<MpNormalizedRectVector, MpNormalizedRect, mediapipe::NormalizedRect>(packet);
}
